using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Dummiesman;
using UnityEngine;

namespace Dummiesman
{
    public class ZipMaterialLoader : MTLLoader
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");
        private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");

        private readonly Dictionary<string, ZipArchiveEntry> zipMap;
        private List<MaterialData> mtlList;

        public Dictionary<string, Material> Materials => mtlList?.ToDictionary(materialData => materialData.name,
            materialData => materialData.Build(this)); // blocking

        public IEnumerator SetMaterials(Action<Dictionary<string, Material>> action, int perFrame = 5)
        {
            var materials = new Dictionary<string, Material>();
            var buildCount = 0;
            foreach (var item in mtlList)
            {
                materials.Add(item.name, item.Build(this));
                if (++buildCount < perFrame) continue;
                buildCount = 0;
                yield return null;
            }

            action.Invoke(materials);
        }

        public ZipMaterialLoader(Dictionary<string, ZipArchiveEntry> zipMap)
        {
            this.zipMap = zipMap;
        }

        public void Load(Stream input)
        {
            var inputReader = new StreamReader(input);
            var reader = new StringReader(inputReader.ReadToEnd());

            mtlList = new List<MaterialData>();
            MaterialData currentMaterial = null;

            for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string processedLine = line.Clean();
                string[] splitLine = processedLine.Split(' ');

                if (splitLine.Length < 2 || processedLine[0] == '#')
                    continue;

                if (splitLine[0] == "newmtl")
                {
                    var materialName = processedLine.Substring(7);
                    var newMtl = new MaterialData
                    {
                        name = materialName
                    };
                    mtlList.Add(newMtl);
                    currentMaterial = newMtl;
                    continue;
                }

                if (currentMaterial == null) continue;

                if (splitLine[0] == "Kd" || splitLine[0] == "kd")
                {
                    currentMaterial.kd = splitLine;
                    continue;
                }

                if (splitLine[0] == "map_Kd" || splitLine[0] == "map_kd")
                {
                    var texturePath = GetTexturePath(processedLine, splitLine);
                    if (texturePath != null)
                    {
                        currentMaterial.mapKdTexturePath = texturePath;
                        currentMaterial.mapKdTextureBytes = LoadTextureBytes(texturePath);
                    }

                    continue;
                }

                if (splitLine[0] == "map_Bump" || splitLine[0] == "map_bump")
                {
                    var texturePath = GetTexturePath(processedLine, splitLine);
                    if (texturePath != null)
                    {
                        currentMaterial.mapBumpTexturePath = texturePath;
                        currentMaterial.mapBumpTextureBytes = LoadTextureBytes(texturePath);
                        currentMaterial.mapBumpScale = GetArgValue(splitLine, "-bm", 1.0f);
                    }

                    continue;
                }

                if (splitLine[0] == "Ks" || splitLine[0] == "ks")
                {
                    currentMaterial.ks = splitLine;
                    continue;
                }

                if (splitLine[0] == "Ka" || splitLine[0] == "ka")
                {
                    currentMaterial.ka = splitLine;
                    continue;
                }

                if (splitLine[0] == "map_Ka" || splitLine[0] == "map_ka")
                {
                    var texturePath = GetTexturePath(processedLine, splitLine);
                    if (texturePath != null)
                    {
                        currentMaterial.mapKaTexturePath = texturePath;
                        currentMaterial.mapKaTextureBytes = LoadTextureBytes(texturePath);
                    }

                    continue;
                }

                if (splitLine[0] == "d" || splitLine[0] == "Tr")
                {
                    currentMaterial.tr = splitLine;
                    continue;
                }

                if (splitLine[0] == "Ns" || splitLine[0] == "ns")
                {
                    currentMaterial.ns = splitLine;
                }
            }
        }

        private byte[] LoadTextureBytes(string fn)
        {
            if (!zipMap.ContainsKey(fn)) return null;
            using var stream = zipMap[fn].Open();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private string GetTexturePath(string processedLine, string[] splitLine)
        {
            var texNameCmpIdx = GetTexNameIndex(splitLine);
            if (texNameCmpIdx < 0)
                return null;
            var texNameIdx = processedLine.IndexOf(splitLine[texNameCmpIdx]);
            var texturePath = processedLine.Substring(texNameIdx);
            var separator = Path.DirectorySeparatorChar.ToString().Equals("/") ? "/" : "\\\\";
            texturePath = new Regex(@"\\\\").Replace(texturePath, separator);
            texturePath = new Regex(@"/").Replace(texturePath, separator);
            return texturePath;
        }

        private class MaterialData
        {
            public string name;

            public string[] kd;
            public string[] ks;
            public string[] ka;
            public string[] tr;
            public string[] ns;

            public byte[] mapKdTextureBytes;
            public string mapKdTexturePath;

            public byte[] mapKaTextureBytes;
            public string mapKaTexturePath;

            public byte[] mapBumpTextureBytes;
            public string mapBumpTexturePath;
            public float mapBumpScale;

            public Material Build(ZipMaterialLoader loader)
            {
                // name
                var material = new Material(Shader.Find("Standard (Specular setup)")) {name = name};
                Color currentColor;

                // kd
                if (kd != null)
                {
                    currentColor = material.GetColor(Color1);
                    var kdColor = OBJLoaderHelper.ColorFromStrArray(kd, 4f);
                    material.SetColor(Color1, new Color(kdColor.r, kdColor.g, kdColor.b, currentColor.a));
                }

                // mapKd
                if (mapKdTexturePath != null)
                {
                    var kdTexture = ImageLoader.LoadTextureFromBytes(mapKdTextureBytes, mapKdTexturePath);
                    material.SetTexture(MainTex, kdTexture);

                    //set transparent mode if the texture has transparency
                    if (kdTexture != null && (kdTexture.format == TextureFormat.DXT5 ||
                                              kdTexture.format == TextureFormat.ARGB32))
                    {
                        OBJLoaderHelper.EnableMaterialTransparency(material);
                    }

                    //flip texture if this is a dds
                    if (Path.GetExtension(mapKdTexturePath).ToLower() == ".dds")
                    {
                        material.mainTextureScale = new Vector2(1f, -1f);
                    }
                }

                // mapBump
                if (mapBumpTexturePath != null)
                {
                    var bumpTexture = ImageLoader.LoadTextureFromBytes(mapBumpTextureBytes, mapBumpTexturePath);
                    bumpTexture = ImageUtils.ConvertToNormalMap(bumpTexture);
                    if (bumpTexture != null)
                    {
                        material.SetTexture(BumpMap, bumpTexture);
                        material.SetFloat(BumpScale, mapBumpScale);
                        material.EnableKeyword("_NORMALMAP");
                    }
                }

                // ks
                if (ks != null)
                    material.SetColor(SpecColor, OBJLoaderHelper.ColorFromStrArray(ks));

                // ka
                if (ka != null)
                {
                    material.SetColor(EmissionColor, OBJLoaderHelper.ColorFromStrArray(ka, 0.05f));
                    material.EnableKeyword("_EMISSION");
                }

                // mapKa
                if (mapKaTexturePath != null)
                {
                    material.SetTexture(EmissionMap,
                        ImageLoader.LoadTextureFromBytes(mapKaTextureBytes, mapKaTexturePath));
                }


                // tr
                if (tr != null)
                {
                    var visibility = OBJLoaderHelper.FastFloatParse(tr[1]);
                    if (tr[0] == "Tr")
                        visibility = 1f - visibility;
                    if (visibility < (1f - Mathf.Epsilon))
                    {
                        currentColor = material.GetColor(Color1);
                        currentColor.a = visibility;
                        material.SetColor(Color1, currentColor);
                        OBJLoaderHelper.EnableMaterialTransparency(material);
                    }
                }


                // ns
                if (ns != null)
                {
                    var Ns = OBJLoaderHelper.FastFloatParse(ns[1]);
                    Ns = (Ns / 1000f);
                    material.SetFloat(Glossiness, Ns);
                }

                return material;
            }
        }
    }
}