using System;
using Siccity.GLTFUtility;

namespace Source.Ui.Utils
{
    public static class ResourcePaths
    {
        public static string ForType(Type type)
        {
            var fullName = type.FullName;
            var index = fullName.IndexOf("`", StringComparison.Ordinal);
            if (index >= 0)
                fullName = fullName.Substring(0, index);

            var parts = fullName?.Split(".");
            if (parts == null || parts.Length <= 1)
                throw new ArgumentException("Invalid class fullname: " + fullName);
            return string.Join("/", parts.SubArray(1, parts.Length - 1));
        }
    }
}