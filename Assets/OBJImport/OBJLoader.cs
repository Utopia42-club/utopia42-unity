﻿/*
 * Copyright (c) 2019 Dummiesman
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
*/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dummiesman
{
    public enum SplitMode
    {
        None,
        Object,
        Material
    }

    public class OBJLoader
    {
        //options
        /// <summary>
        /// Determines how objects will be created
        /// </summary>
        private SplitMode splitMode = SplitMode.Object;

        //accessed by builder
        internal readonly List<Vector3> vertices = new List<Vector3>();
        internal readonly List<Vector3> normals = new List<Vector3>();
        internal readonly List<Vector2> uVs = new List<Vector2>();
        internal Dictionary<string, Material> materials;

        //file info for files loaded from file path, used for GameObject naming and MTL finding
        private FileInfo objInfo;

        private Dictionary<string, ZipArchiveEntry> zipMap;
        private Dictionary<string, OBJObjectBuilder> builderDictionary;

#if UNITY_EDITOR
        [MenuItem("GameObject/Import From OBJ")]
        static void ObjLoadMenu()
        {
            string pth = EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
            if (!string.IsNullOrEmpty(pth))
            {
                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                s.Start();

                var loader = new OBJLoader
                {
                    splitMode = SplitMode.Object,
                };
                loader.Load(pth);

                Debug.Log($"OBJ import time: {s.ElapsedMilliseconds}ms");
                s.Stop();
            }
        }
#endif

        /// <summary>
        /// Helper function to load mtllib statements
        /// </summary>
        /// <param name="mtlLibPath"></param>
        private void LoadMaterialLibrary(string mtlLibPath)
        {
            if (zipMap != null)
            {
                if (!zipMap.ContainsKey(mtlLibPath)) return;
                using var stream = zipMap[mtlLibPath].Open();
                materials = new MTLLoader(zipMap).Load(stream);
                return;
            }

            if (objInfo != null)
            {
                if (File.Exists(Path.Combine(objInfo.Directory.FullName, mtlLibPath)))
                {
                    materials = new MTLLoader().Load(Path.Combine(objInfo.Directory.FullName, mtlLibPath));
                    return;
                }
            }

            if (File.Exists(mtlLibPath))
            {
                materials = new MTLLoader().Load(mtlLibPath);
            }
        }

        /// <summary>
        /// Load an OBJ file from a stream. No materials will be loaded, and will instead be supplemented by a blank white material.
        /// </summary>
        /// <param name="input">Input OBJ stream</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(Stream input)
        {
            CreateBuilderDictionary(input);
            return BuildBuilderDictionary();
        }

        public GameObject BuildBuilderDictionary()
        {
            if (builderDictionary == null || builderDictionary.Count == 0) return null;
            //finally, put it all together
            GameObject obj =
                new GameObject(objInfo != null
                    ? Path.GetFileNameWithoutExtension(objInfo.Name)
                    : "WavefrontObject"); // TODO: change the name
            obj.transform.localScale = new Vector3(1f, 1f, 1f);

            foreach (var builder in builderDictionary)
            {
                //empty object
                if (builder.Value.PushedFaceCount == 0)
                    continue;
                var builtObj = builder.Value.Build();
                builtObj.transform.SetParent(obj.transform, false);
            }

            return obj;
        }

        public void CreateBuilderDictionary(Stream input = null)
        {
            var reader = new StreamReader(input ?? zipMap["obj"].Open());
            //var reader = new StringReader(inputReader.ReadToEnd());

            builderDictionary = new Dictionary<string, OBJObjectBuilder>();
            OBJObjectBuilder currentBuilder = null;
            string currentMaterial = "default";

            //lists for face data
            //prevents excess GC
            List<int> vertexIndices = new List<int>();
            List<int> normalIndices = new List<int>();
            List<int> uvIndices = new List<int>();

            //helper func
            Action<string> setCurrentObjectFunc = objectName =>
            {
                if (!builderDictionary.TryGetValue(objectName, out currentBuilder))
                {
                    currentBuilder = new OBJObjectBuilder(objectName, this);
                    builderDictionary[objectName] = currentBuilder;
                }
            };

            //create default object
            setCurrentObjectFunc.Invoke("default");

            //var buffer = new DoubleBuffer(reader, 256 * 1024);
            var buffer = new CharWordReader(reader, 4 * 1024);

            //do the reading
            while (true)
            {
                buffer.SkipWhitespaces();

                if (buffer.endReached)
                {
                    break;
                }

                buffer.ReadUntilWhiteSpace();

                //comment or blank
                if (buffer.Is("#"))
                {
                    buffer.SkipUntilNewLine();
                    continue;
                }

                if (materials == null && buffer.Is("mtllib"))
                {
                    buffer.SkipWhitespaces();
                    buffer.ReadUntilNewLine();
                    string mtlLibPath = buffer.GetString();
                    LoadMaterialLibrary(mtlLibPath);
                    continue;
                }

                if (buffer.Is("v"))
                {
                    vertices.Add(buffer.ReadVector());
                    continue;
                }

                //normal
                if (buffer.Is("vn"))
                {
                    normals.Add(buffer.ReadVector());
                    continue;
                }

                //uv
                if (buffer.Is("vt"))
                {
                    uVs.Add(buffer.ReadVector());
                    continue;
                }

                //new material
                if (buffer.Is("usemtl"))
                {
                    buffer.SkipWhitespaces();
                    buffer.ReadUntilNewLine();
                    string materialName = buffer.GetString();
                    currentMaterial = materialName;

                    if (splitMode == SplitMode.Material)
                    {
                        setCurrentObjectFunc.Invoke(materialName);
                    }

                    continue;
                }

                //new object
                if ((buffer.Is("o") || buffer.Is("g")) && splitMode == SplitMode.Object)
                {
                    buffer.ReadUntilNewLine();
                    string objectName = buffer.GetString(1);
                    setCurrentObjectFunc.Invoke(objectName);
                    continue;
                }

                //face data (the fun part)
                if (buffer.Is("f"))
                {
                    //loop through indices
                    while (true)
                    {
                        bool newLinePassed;
                        buffer.SkipWhitespaces(out newLinePassed);
                        if (newLinePassed)
                        {
                            break;
                        }

                        int vertexIndex = int.MinValue;
                        int normalIndex = int.MinValue;
                        int uvIndex = int.MinValue;

                        vertexIndex = buffer.ReadInt();
                        if (buffer.currentChar == '/')
                        {
                            buffer.MoveNext();
                            if (buffer.currentChar != '/')
                            {
                                uvIndex = buffer.ReadInt();
                            }

                            if (buffer.currentChar == '/')
                            {
                                buffer.MoveNext();
                                normalIndex = buffer.ReadInt();
                            }
                        }

                        //"postprocess" indices
                        if (vertexIndex > int.MinValue)
                        {
                            if (vertexIndex < 0)
                                vertexIndex = vertices.Count - vertexIndex;
                            vertexIndex--;
                        }

                        if (normalIndex > int.MinValue)
                        {
                            if (normalIndex < 0)
                                normalIndex = normals.Count - normalIndex;
                            normalIndex--;
                        }

                        if (uvIndex > int.MinValue)
                        {
                            if (uvIndex < 0)
                                uvIndex = uVs.Count - uvIndex;
                            uvIndex--;
                        }

                        //set array values
                        vertexIndices.Add(vertexIndex);
                        normalIndices.Add(normalIndex);
                        uvIndices.Add(uvIndex);
                    }

                    //push to builder
                    currentBuilder.PushFace(currentMaterial, vertexIndices, normalIndices, uvIndices);

                    //clear lists
                    vertexIndices.Clear();
                    normalIndices.Clear();
                    uvIndices.Clear();

                    continue;
                }

                buffer.SkipUntilNewLine();
            }
        }

        /// <summary>
        /// Load an OBJ and MTL file from a stream.
        /// </summary>
        /// <param name="input">Input OBJ stream</param>
        /// /// <param name="mtlInput">Input MTL stream</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(Stream input, Stream mtlInput)
        {
            var mtlLoader = new MTLLoader();
            materials = mtlLoader.Load(mtlInput);

            return Load(input);
        }

        /// <summary>
        /// Load an OBJ and MTL file from a file path.
        /// </summary>
        /// <param name="path">Input OBJ path</param>
        /// /// <param name="mtlPath">Input MTL path</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(string path, string mtlPath)
        {
            objInfo = new FileInfo(path);
            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {
                var mtlLoader = new MTLLoader();
                materials = mtlLoader.Load(mtlPath);

                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return Load(fs);
                }
            }
            else
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return Load(fs);
                }
            }
        }

        /// <summary>
        /// Load an OBJ file from a file path. This function will also attempt to load the MTL defined in the OBJ file.
        /// </summary>
        /// <param name="path">Input OBJ path</param>
        /// <returns>Returns a GameObject representing the OBJ file, with each imported object as a child.</returns>
        public GameObject Load(string path)
        {
            return Load(path, null);
        }

        private void LoadZip(ZipArchive zip)
        {
            InitZipMap(zip);
            InitMaterialsForZip();
        }

        public void InitMaterialsForZip()
        {
            if (zipMap.ContainsKey("mtl"))
            {
                using var stream = zipMap["mtl"].Open();
                materials = new MTLLoader(zipMap).Load(stream);
            }
        }

        public void InitZipMap(ZipArchive zip)
        {
            var separator = Path.DirectorySeparatorChar.ToString().Equals("/") ? "/" : "\\\\";
            zipMap = new Dictionary<string, ZipArchiveEntry>();
            List<ZipArchiveEntry> entries = new List<ZipArchiveEntry>(zip.Entries);
            entries.Sort(new SortArchiveEntries());
            foreach (var entry in entries)
            {
                if (entry.FullName.StartsWith("__MACOSX")) continue;

                var fullName = new Regex(@"\\\\").Replace(entry.FullName, separator);
                fullName = new Regex(@"/").Replace(fullName, separator);
                if (fullName.EndsWith(separator)) continue;

                if (zipMap.ContainsKey(fullName) || fullName.Equals("")) continue;

                zipMap.Add(fullName, entry);

                var modifiedFullname =
                    new Regex(separator.Equals("/") ? @"^[\S\s]+?/" : @"^[\S\s]+?\\\\").Replace(fullName, "");
                if (!zipMap.ContainsKey(modifiedFullname))
                    zipMap.Add(modifiedFullname, entry);

                var type = entry.Name.Split('.').Last().ToLower();
                if ((type.Equals("mtl") || type.Equals("obj")) && !zipMap.ContainsKey(type))
                    zipMap.Add(type, entry);
            }
            if (!zipMap.ContainsKey("obj")) throw new InvalidDataException("Obj file not found");
        }

        public GameObject LoadZip(Stream zip) // not thread safe
        {
            using var zipFile = new ZipArchive(zip);
            LoadZip(zipFile);
            CreateBuilderDictionary();
            return BuildBuilderDictionary();
        }
    }
}

class SortArchiveEntries : IComparer<ZipArchiveEntry>
{
    public int Compare(ZipArchiveEntry x, ZipArchiveEntry y)
    {
        return String.Compare(x?.FullName, y?.FullName, StringComparison.Ordinal);
    }
}