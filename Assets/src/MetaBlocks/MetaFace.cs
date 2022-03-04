using System;
using src.Utils;
using UnityEngine;

namespace src
{
    public class MetaFace : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private MeshRenderer meshRenderer;

        public MeshRenderer Initialize(Voxels.Face face, int width, int height)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));

            if (face == Voxels.Face.FRONT || face == Voxels.Face.BACK)
                transform.localScale = new Vector3(width, height, 1);
            else if (face == Voxels.Face.LEFT || face == Voxels.Face.RIGHT)
                transform.localScale = new Vector3(1, height, width);
            else
                transform.localScale = new Vector3(width, 1, height);

            var vertices = new Vector3[4];
            for (var i = 0; i < 4; i++)
                vertices[i] = Voxels.Vertices[face.verts[i]];

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = new int[12] {0, 1, 2, 2, 1, 3, 2, 1, 0, 3, 1, 2,}
            };

            var uv = new Vector2[4]
            {
                //  new Vector2(1, 0),
                // new Vector2(1, 1),
                // new Vector2(0, 0),
                // new Vector2(0, 1)
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };
            mesh.uv = uv;

            Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;
            meshFilter.sharedMesh = mesh;

            meshCollider.convex = true;
            meshCollider.sharedMesh = mesh;

            return meshRenderer;
        }

        protected void OnDestroy()
        {
            if (meshFilter != null)
                Destroy(meshFilter.sharedMesh);

            if (meshRenderer != null)
            {
                Destroy(meshRenderer.sharedMaterial.mainTexture);
                Destroy(meshRenderer.sharedMaterial);
            }
        }
    }
}