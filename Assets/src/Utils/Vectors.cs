using UnityEngine;

public static class Vectors
{

    public static Vector3Int FloorToInt(Vector3 vec)
    {
        return new Vector3Int(
            Mathf.FloorToInt(vec.x),
            Mathf.FloorToInt(vec.y),
            Mathf.FloorToInt(vec.z)
            );
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
