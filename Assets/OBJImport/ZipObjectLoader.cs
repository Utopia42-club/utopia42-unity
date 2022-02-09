using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dummiesman
{
    public class ZipObjectLoader : OBJLoader
    {
        private Dictionary<string, ZipArchiveEntry> zipMap = new Dictionary<string, ZipArchiveEntry>();
        private string mtlLibPath = null;
        private ZipMaterialLoader zipMaterialLoader;
        private ZipArchive zipFile;

        protected override GameObject BuildBuilderDictionary()
        {
            materials = zipMaterialLoader?.Materials;
            return base.BuildBuilderDictionary();
        }

        protected override void LoadMaterialLibrary(string libPath)
        {
            if (!zipMap.ContainsKey(libPath)) return;
            using var stream = zipMap[libPath].Open();
            zipMaterialLoader = new ZipMaterialLoader(zipMap);
            zipMaterialLoader.Load(stream);
        }

        private void LoadMaterial()
        {
            LoadMaterialLibrary(mtlLibPath ?? "mtl");
        }

        private void InitZipMap(ZipArchive zip)
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

        public void Init(Stream zip) // thread safe
        {
            zipFile = new ZipArchive(zip);
            InitZipMap(zipFile);
            CreateBuilderDictionary(zipMap["obj"].Open(), out mtlLibPath, false);
            LoadMaterial();
        }

        public GameObject BuildObject() // not thread safe
        {
            materials = zipMaterialLoader?.Materials;
            var obj = BuildBuilderDictionary();
            zipFile?.Dispose();
            return obj;
        }
    }
}


internal class SortArchiveEntries : IComparer<ZipArchiveEntry>
{
    public int Compare(ZipArchiveEntry x, ZipArchiveEntry y)
    {
        return string.Compare(x?.FullName, y?.FullName, StringComparison.Ordinal);
    }
}