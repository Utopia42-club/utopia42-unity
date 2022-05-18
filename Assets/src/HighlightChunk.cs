using System.Collections.Generic;
using System.Linq;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src
{
    public class HighlightChunk : MonoBehaviour
    {
        private readonly Dictionary<Vector3Int, Transform> selectedBlocks = new Dictionary<Vector3Int, Transform>();
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Chunk chunk;
        private World world;
        private bool started = false;

        private void Start()
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = world.SelectedBlock;
            UpdateHighlights(false);
            started = true;
        }

        public void UpdateHighlights(bool destroyFirst = true)
        {
            if (destroyFirst)
                Clean();
            var chunkSelectedPositions = world.GetChunkSelectedPositions(chunk.coordinate);
            if (chunkSelectedPositions == null) OnChanged();
            else
                Add(chunkSelectedPositions.Select(localPos => new VoxelPosition(chunk.coordinate, localPos)).ToList(),
                    true);
        }

        public static HighlightChunk Create(GameObject highlightObject, World world, Chunk chunk)
        {
            var highlightChunk = highlightObject.AddComponent<HighlightChunk>();
            highlightChunk.world = world;
            highlightChunk.chunk = chunk;
            return highlightChunk;
        }

        private void OnChanged()
        {
            // TODO: redraw neighbors? here or in Chunk class?
            DestroyMeshAndMaterial();
            DrawHighlights();
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
            foreach (var pos in selectedBlocks.Keys)
            {
                AddVisibleFaces(pos + world.GetHighlightOffset(new VoxelPosition(chunk.coordinate, pos)), vertices,
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

        public void UpdateMetaHighlight(VoxelPosition vp)
        {
            if (!selectedBlocks.TryGetValue(vp.local, out var metaBlockHighlight)) return;
            if (metaBlockHighlight != null)
                DestroyImmediate(metaBlockHighlight);
            selectedBlocks[vp.local] = GetMetaBlockHighlight(vp);
        }

        private Transform GetMetaBlockHighlight(VoxelPosition vp)
        {
            return chunk.GetMetaAt(vp)?.blockObject.CreateSelectHighlight();
        }

        // private void Add(VoxelPosition vp)
        // {
        //     if (TryAdd(vp))
        //         OnChanged();
        // }

        private void Add(List<VoxelPosition> vps, bool forceRedraw = false)
        {
            var changed = false;
            foreach (var vp in vps)
                if (TryAdd(vp))
                    changed = true;
            if (forceRedraw || changed)
                OnChanged();
        }

        private bool TryAdd(VoxelPosition vp)
        {
            var blockType = chunk.GetBlock(vp.local);
            if (!blockType.isSolid) return false;
            // if (!TryRemove(vp)) // remove and do not add if it already exists
            // {
            var metaHighlight = GetMetaBlockHighlight(vp);
            if (metaHighlight != null)
                metaHighlight.position = metaHighlight.position + world.GetHighlightOffset(vp);
            selectedBlocks.Add(vp.local, metaHighlight);
            // }
            return true;
        }

        // private void Remove(VoxelPosition vp)
        // {
        //     if (TryRemove(vp))
        //         OnChanged();
        // }
        //
        // private void Remove(List<VoxelPosition> vps)
        // {
        //     var changed = false;
        //     foreach (var vp in vps)
        //         if (TryRemove(vp))
        //             changed = true;
        //     if (changed) OnChanged();
        // }

        // private bool TryRemove(VoxelPosition vp)
        // {
        //     if (!selectedBlocks.TryGetValue(vp.local, out var metaHighlight)) return false;
        //     if (metaHighlight != null)
        //         Destroy(metaHighlight.gameObject);
        //     selectedBlocks.Remove(vp.local);
        //     return true;
        // }

        private void Clean()
        {
            DestroyMeshAndMaterial();
            foreach (var metaHighlight in selectedBlocks.Values.Where(v => v != null))
                Destroy(metaHighlight.gameObject);
            selectedBlocks.Clear();
        }

        private void DestroyMeshAndMaterial()
        {
            // Destroy(meshRenderer.sharedMaterial); // Destroying assets not allowed
            Destroy(meshFilter.sharedMesh);
        }

        private void OnDisable()
        {
            Clean();
        }

        private void OnEnable() // in case a garbage chunk gets activated again
        {
            if (!started) return;
            UpdateHighlights(false);
        }
    }
}