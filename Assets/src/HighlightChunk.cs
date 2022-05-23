using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src
{
    public class HighlightChunk : MonoBehaviour
    {
        private readonly Dictionary<Vector3Int, HighlightedBlock> highlightedBlocks =
            new Dictionary<Vector3Int, HighlightedBlock>(); // local coordinate -> selected block

        public Vector3Int Position { get; private set; }

        // TODO: The parent of each selected block should be highlight chunk so that they would be destroyed with it   
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        public GameObject HighlightChunkGameObject { get; private set; }

        public Transform transform => HighlightChunkGameObject.transform;
        
        public int TotalBlocksHighlighted => highlightedBlocks.Count(pair => pair.Value != null);
        public List<HighlightedBlock> HighlightedBlocks => new List<HighlightedBlock>(highlightedBlocks.Values);
        public HashSet<Vector3Int> HighlightedLocalPositions => new HashSet<Vector3Int>(highlightedBlocks.Keys);
        public bool SelectionDisplaced =>
            highlightedBlocks.Values.Any(highlightedBlock => highlightedBlock.Offset != Vector3Int.zero);
        private void Start()
        {
            meshFilter = HighlightChunkGameObject.AddComponent<MeshFilter>();
            meshRenderer = HighlightChunkGameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = World.INSTANCE.SelectedBlock;
        }
        
        public static HighlightChunk Create(GameObject parent, Vector3Int coordinate)
        {
            var highlightChunk = parent.AddComponent<HighlightChunk>(); // TODO: necessary?
            highlightChunk.HighlightChunkGameObject = new GameObject
            {
                name = "Highlight Chunk " + coordinate
            };
            var transform = highlightChunk.transform;
            transform.SetParent(parent.transform);
            var position = coordinate;
            position.Scale(Chunk.CHUNK_SIZE);
            transform.localPosition = position;
            highlightChunk.Position = position;
            return highlightChunk;
        }
        
        public bool Contains(Vector3Int localPos)
        {
            return highlightedBlocks.ContainsKey(localPos);
        }

        public void Add(Vector3Int localPos, HighlightedBlock highlightedBlock)
        {
            if (highlightedBlocks.ContainsKey(localPos))
            {
                Debug.LogError("HighlightChunk already contains given selected position");
                return;
            }

            highlightedBlocks.Add(localPos, highlightedBlock);
        }

        public bool Remove(Vector3Int localPos)
        {
            if (highlightedBlocks.TryGetValue(localPos, out var highlightedBlock) && highlightedBlock != null)
                DestroyImmediate(highlightedBlock);
            return highlightedBlocks.Remove(localPos);
        }

        public void Rotate(Vector3 center, Vector3 axis)
        {
            foreach (var highlightedBlock in highlightedBlocks.Values)
                highlightedBlock.Rotate(center, axis, Position);
        }

        public Vector3Int? GetRotationOffset(Vector3Int localPos)
        {
            if (!highlightedBlocks.TryGetValue(localPos, out var highlightedBlock) ||
                highlightedBlock == null) return null;
            return highlightedBlock.Offset;
        }

        public void Redraw()
        {
            // TODO: redraw neighbors? here or in Chunk class?
            DestroyMeshAndMaterial();
            DrawHighlights();
            foreach (var highlightedBlock in highlightedBlocks.Values)
                highlightedBlock.UpdateMetaBlockHighlightPosition();
        }

        private void DrawHighlights()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            CreateMeshData(vertices, triangles, uvs);
            CreateMesh(vertices, triangles, uvs);
        }

        private void CreateMeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            foreach (var pos in highlightedBlocks.Keys)
            {
                var highlightedBlock = highlightedBlocks[pos];
                if (highlightedBlock != null)
                    AddVisibleFaces(highlightedBlock.CurrentPosition, vertices,
                        triangles, uvs);
            }
        }

        private void AddVisibleFaces(Vector3Int pos, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            Vector3Int[] verts =
            {
                Voxels.Vertices[0] + pos,
                Voxels.Vertices[1] + pos,
                Voxels.Vertices[2] + pos,
                Voxels.Vertices[3] + pos,
                Voxels.Vertices[4] + pos,
                Voxels.Vertices[5] + pos,
                Voxels.Vertices[6] + pos,
                Voxels.Vertices[7] + pos
            };

            foreach (var face in Voxels.Face.FACES)
            {
                var idx = vertices.Count;

                vertices.Add(verts[face.verts[0]]);
                vertices.Add(verts[face.verts[1]]);
                vertices.Add(verts[face.verts[2]]);
                vertices.Add(verts[face.verts[3]]);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1));

                triangles.Add(idx);
                triangles.Add(idx + 1);
                triangles.Add(idx + 2);
                triangles.Add(idx + 2);
                triangles.Add(idx + 1);
                triangles.Add(idx + 3);
            }
        }

        private void CreateMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.Optimize();

            Destroy(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;
        }

        private void DestroyMeshAndMaterial()
        {
            // Destroy(meshRenderer.sharedMaterial); // Destroying assets not allowed
            if (meshFilter.sharedMesh == null) return;
            Destroy(meshFilter.sharedMesh);
            meshFilter.sharedMesh = null;
        }

        private void OnDestroy()
        {
            DestroyMeshAndMaterial();
            Destroy(HighlightChunkGameObject);
        }
    }
}