using UnityEngine;

namespace src.Utils
{
    public static class Vectors
    {
        public static Vector3Int FloorToInt(float x, float y, float z)
        {
            return new Vector3Int(
                Mathf.FloorToInt(x),
                Mathf.FloorToInt(y),
                Mathf.FloorToInt(z)
            );
        }

        public static Vector3Int FloorToInt(Vector3 vec)
        {
            return FloorToInt(vec.x, vec.y, vec.z);
        }

        public static Vector3Int TruncateFloor(float x, float y, float z, int truncatePrecision = 3)
        {
            return TruncateFloor(new Vector3(x, y, z), truncatePrecision);
        }

        public static Vector3Int TruncateFloor(Vector3 vector, int truncatePrecision = 3, int multiplyPower = 0)
        {
            var constant = Mathf.Pow(10, truncatePrecision);
            var multiply = Mathf.Pow(10, multiplyPower - truncatePrecision);
            vector.x = Mathf.Round(vector.x * constant) * multiply;
            vector.y = Mathf.Round(vector.y * constant) * multiply;
            vector.z = Mathf.Round(vector.z * constant) * multiply;
            return FloorToInt(vector);
        }
        
        public static Vector3 Truncate(float x, float y, float z, int truncatePrecision = 3)
        {
            return Truncate(new Vector3(x, y, z), truncatePrecision);
        }

        public static Vector3 Truncate(Vector3 vector, int truncatePrecision = 3)
        {
            var constant = Mathf.Pow(10, truncatePrecision);
            vector.x = Mathf.Round(vector.x * constant) / constant;
            vector.y = Mathf.Round(vector.y * constant) / constant;
            vector.z = Mathf.Round(vector.z * constant) / constant;
            return vector;
        }

        public static Vector3Int ParseKey(string key)
        {
            string[] xyz = key.Split('_');
            if (xyz.Length != 3)
                throw new System.FormatException("Invalid cordinate key: " + key);
            return new Vector3Int(int.Parse(xyz[0]), int.Parse(xyz[1]), int.Parse(xyz[2]));
        }

        public static string FormatKey(Vector3Int pos)
        {
            return FormatKey(pos.x, pos.y, pos.z);
        }

        public static string FormatKey(int x, int y, int z)
        {
            return string.Format("{0}_{1}_{2}", x, y, z);
        }
    }
}