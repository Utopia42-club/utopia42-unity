using System;
using UnityEngine;

public static class Voxels
{
    public class Face
    {
        public readonly Vector3Int direction;
        public readonly int[] triangles;
        public readonly int index;
        private Face(int index, Vector3Int dir, int[] tris)
        {
            this.index = index;
            direction = dir;
            triangles = tris;
            if (index < 0 || index > 6 || FACES[index] != null)
                throw new ArgumentException(String.Format("Invalid index: {0}", index));
            FACES[index] = this;
        }

        public static Face[] FACES = new Face[6];
        public static Face BACK = new Face(0, Vector3Int.back, new int[] { 0, 2, 4, 4, 2, 6 });
        public static Face RIGHT = new Face(1, Vector3Int.right, new int[] { 4, 6, 5, 5, 6, 7 });
        public static Face FRONT = new Face(2, Vector3Int.forward, new int[] { 3, 1, 5, 5, 7, 3 });
        public static Face LEFT = new Face(3, Vector3Int.left, new int[] { 3, 2, 1, 1, 2, 0 });
        public static Face BOTTOM = new Face(4, Vector3Int.down, new int[] { 0, 4, 1, 4, 5, 1 });
        public static Face TOP = new Face(5, Vector3Int.up, new int[] { 2, 3, 6, 6, 3, 7 });
    }

    public static Vector3Int[] GenereateVertices()
    {
        return GenereateVertices(Vector3Int.zero);
    }

    public static Vector3Int[] GenereateVertices(Vector3Int offset)
    {
        var result = new Vector3Int[8];
        var bin = new int[] { 0, 1 };
        int idx = 0;
        foreach (int x in bin)
            foreach (int y in bin)
                foreach (int z in bin)
                    result[idx++] = new Vector3Int(x, y, z) + offset;
        return result;
    }
}
