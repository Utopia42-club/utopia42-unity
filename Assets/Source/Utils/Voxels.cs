using System;
using UnityEngine;

namespace Source.Utils
{
    public static class Voxels
    {
        public static readonly int TextureAtlasSizeInBlocks = 6;
        public static readonly float NormalizedBlockTextureSize = 1f / (float)TextureAtlasSizeInBlocks;


        public class Face
        {
            public readonly Vector3Int direction;
            public readonly int[] verts;
            public readonly int index;

            // Note that the order of verts is important
            // Verts need to be ordered such that triangles are: 1,2,2,1,3
            private Face(int index, Vector3Int dir, int[] verts)
            {
                this.index = index;
                direction = dir;
                this.verts = verts;
                if (index < 0 || index > 6 || FACES[index] != null)
                    throw new ArgumentException(String.Format("Invalid index: {0}", index));
                FACES[index] = this;
            }

            public static Face[] FACES = new Face[6];
            public static Face FRONT = new Face(0, Vector3Int.forward, new int[] { 5, 6, 4, 7 });
            public static Face RIGHT = new Face(1, Vector3Int.right, new int[] { 1, 2, 5, 6 });
            public static Face BACK = new Face(2, Vector3Int.back, new int[] { 0, 3, 1, 2 });
            public static Face LEFT = new Face(3, Vector3Int.left, new int[] { 4, 7, 0, 3 });
            public static Face BOTTOM = new Face(4, Vector3Int.down, new int[] { 4, 0, 5, 1 });
            public static Face TOP = new Face(5, Vector3Int.up, new int[] { 3, 7, 2, 6 });
        }

        public static readonly Vector3Int[] Vertices = new Vector3Int[8] {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 1, 1),
            new Vector3Int(0, 1, 1),
        };
    }
}
