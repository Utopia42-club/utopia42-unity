/*
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
using System.Collections;
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
        /// <param name="libPath"></param>
        protected virtual void LoadMaterialLibrary(string libPath)
        {
            if (objInfo != null)
            {
                if (File.Exists(Path.Combine(objInfo.Directory.FullName, libPath)))
                {
                    materials = new MTLLoader().Load(Path.Combine(objInfo.Directory.FullName, libPath));
                    return;
                }
            }

            if (File.Exists(libPath))
            {
                materials = new MTLLoader().Load(libPath);
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
            return BuildObject();
        }

        protected GameObject BuildObject() // blocking
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
        
        protected IEnumerator BuildObject(Action<GameObject> onSuccess, int perFrame = 5) // non-blocking
        {
            if (builderDictionary == null || builderDictionary.Count == 0) yield break;
            //finally, put it all together
            GameObject obj =
                new GameObject(objInfo != null
                    ? Path.GetFileNameWithoutExtension(objInfo.Name)
                    : "WavefrontObject"); // TODO: change the name
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.gameObject.SetActive(false);

            var buildCount = 0;
            foreach (var builder in builderDictionary)
            {
                //empty object
                if (builder.Value.PushedFaceCount == 0)
                    continue;
                var builtObj = builder.Value.Build();
                builtObj.transform.SetParent(obj.transform, false);
                if (++buildCount <= perFrame) continue;
                buildCount = 0;
                yield return null;
            }
            obj.gameObject.SetActive(true);
            onSuccess(obj);
        }

        private void CreateBuilderDictionary(Stream input)
        {
            CreateBuilderDictionary(input, out _);
        }

        protected void CreateBuilderDictionary(Stream input, out string mtlLibPath, bool tryLoadMaterial = true)
        {
            if (input == null)
            {
                mtlLibPath = null;
                return;
            }

            var reader = new StreamReader(input);
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

                if (buffer.Is("mtllib") && materials == null)
                {
                    buffer.SkipWhitespaces();
                    buffer.ReadUntilNewLine();
                    mtlLibPath = buffer.GetString();
                    if (tryLoadMaterial)
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

            mtlLibPath = null;
        }

        /// <summary>
        /// Load an OBJ and MTL file from a stream.
        /// </summary>
        /// <param name="input">Input OBJ stream</param>
        /// /// <param name="mtlInput">Input MTL stream</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        protected GameObject Load(Stream input, Stream mtlInput)
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
        private GameObject Load(string path, string mtlPath)
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
    }
}