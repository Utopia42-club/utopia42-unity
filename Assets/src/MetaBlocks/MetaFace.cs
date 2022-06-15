using System;
using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class MetaFace : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private MeshRenderer meshRenderer;

        public MeshRenderer Initialize(bool withCollider)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            if (withCollider)
                meshCollider = gameObject.AddComponent<MeshCollider>();

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));

            var mesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new Vector3Int(0, 0, 0),
                    new Vector3Int(0, 1, 0),
                    new Vector3Int(1, 0, 0),
                    new Vector3Int(1, 1, 0)
                },
                triangles = new int[12] {0, 1, 2, 2, 1, 3, 2, 1, 0, 3, 1, 2,},
                uv = new Vector2[4]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 0),
                    new Vector2(1, 1)
                },
                normals = new Vector3[4]
                {
                    -Vector3.forward,
                    -Vector3.forward,
                    -Vector3.forward,
                    -Vector3.forward
                }
            };

            meshFilter.sharedMesh = mesh;
            if (withCollider)
            {
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = true;
            }

            return meshRenderer;
        }

        protected void OnDestroy()
        {
            if (meshFilter != null)
                Destroy(meshFilter.sharedMesh);

            if (meshRenderer == null) return;
            var mat = meshRenderer.sharedMaterial;
            if (mat == null) return;
            if (mat.mainTexture != null && mat.mainTexture.name != "failed" && mat.mainTexture.name != "video" &&
                mat.mainTexture.name != "nft" && mat.mainTexture.name != "image") // TODO ?
                Destroy(mat.mainTexture);
            Destroy(mat);
        }
    }
}