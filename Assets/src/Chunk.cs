using System;
using System.Collections;
using System.Collections.Generic;
using src.MetaBlocks;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class Chunk
    {
        public static readonly int CHUNK_WIDTH = 16;
        public static readonly int CHUNK_HEIGHT = 32;

        private Dictionary<Vector3Int, MetaBlock> metaBlocks;
        private readonly uint[,,] voxels = new uint[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];
        private World world;
        public GameObject chunkObject;

        public readonly Vector3Int position;
        public readonly Vector3Int coordinate;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;
        private bool inited = false;
        private bool active = true;

        public Chunk(Vector3Int coordinate, World world)
        {
            this.coordinate = coordinate;
            this.world = world;
            this.position = new Vector3Int(coordinate.x * CHUNK_WIDTH, coordinate.y * CHUNK_HEIGHT,
                coordinate.z * CHUNK_WIDTH);
        }

        public bool IsInited()
        {
            return inited;
        }

        public IEnumerator Init()
        {
            if (inited) yield break;
            inited = true;
            chunkObject = new GameObject();
            chunkObject.SetActive(active);
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            meshCollider = chunkObject.AddComponent<MeshCollider>();

            meshRenderer.sharedMaterials = new[]
                {world.material, new Material(Shader.Find("Particles/Standard Surface"))};
            chunkObject.transform.SetParent(world.transform);
            chunkObject.transform.position = position;
            chunkObject.name = "Chunck " + coordinate;

            yield return WorldService.INSTANCE.FillChunk(coordinate, voxels);
            yield return WorldService.INSTANCE.GetMetaBlocksForChunk(coordinate, mb => metaBlocks = mb);
            
            DrawVoxels();
            DrawMetaBlocks();
            //var block = new ImageBlockTpe(50).New("{front:{height:5, width:5, url:\"https://www.wpbeginner.com/wp-content/uploads/2020/03/ultimate-small-business-resource-180x180.png\"}}");
            //block.RenderAt(chunkObject.transform, position+new Vector3Int(0, 32, 0));
        }

        private void DrawMetaBlocks()
        {
            if (metaBlocks != null)
            {
                foreach (var entry in metaBlocks)
                    entry.Value.RenderAt(chunkObject.transform, entry.Key, this);
            }
        }

        private void DrawVoxels()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var coloredTriangles = new List<int>();
            var uvs = new List<Vector2>();
            var colors = new List<Color32>();

            CreateMeshData(vertices, triangles, coloredTriangles, uvs, colors);
            CreateMesh(vertices, triangles, coloredTriangles, uvs, colors);
        }

        void CreateMeshData(List<Vector3> vertices, List<int> triangles, List<int> coloredTriangles, List<Vector2> uvs,
            List<Color32> colors)
        {
            for (int y = 0; y < voxels.GetLength(1); y++)
            for (int x = 0; x < voxels.GetLength(0); x++)
            for (int z = 0; z < voxels.GetLength(2); z++)
                if (WorldService.INSTANCE.GetBlockType(voxels[x, y, z]).isSolid)
                    AddVisibleFaces(new Vector3Int(x, y, z), vertices, triangles, coloredTriangles, uvs, colors);
        }

        // Inputs: x,y,z local to this chunk
        private bool IsVoxelInChunk(int x, int y, int z)
        {
            return x >= 0 && x < voxels.GetLength(0) &&
                   y >= 0 && y < voxels.GetLength(1) &&
                   z >= 0 && z < voxels.GetLength(2);
        }

        public BlockType GetBlock(Vector3Int localPos)
        {
            if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
                throw new ArgumentException("Invalid local position: " + localPos);

            return WorldService.INSTANCE.GetBlockType(voxels[localPos.x, localPos.y, localPos.z]);
        }

        public MetaBlock GetMetaAt(VoxelPosition vp)
        {
            if (metaBlocks == null) return null;

            MetaBlock block;
            if (metaBlocks.TryGetValue(vp.local, out block))
                return block;

            return null;
        }

        private bool IsPositionSolidIfLoaded(Vector3Int localPos)
        {
            if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
                return world.IsSolidIfLoaded(ToGlobal(localPos));

            return GetBlock(localPos).isSolid;
        }

        void AddVisibleFaces(Vector3Int pos, List<Vector3> vertices, List<int> triangles, List<int> coloredTriangles,
            List<Vector2> uvs, List<Color32> colors)
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

            var blockId = voxels[pos.x, pos.y, pos.z];
            var type = WorldService.INSTANCE.GetBlockType(blockId);
            if (!type.isSolid) return;

            var targetTriangles = type.color != null ? coloredTriangles : triangles;
            var color = type.color ?? Color.white;

            foreach (var face in Voxels.Face.FACES)
            {
                if (!IsPositionSolidIfLoaded(pos + face.direction))
                {
                    var idx = vertices.Count;

                    vertices.Add(verts[face.verts[0]]);
                    vertices.Add(verts[face.verts[1]]);
                    vertices.Add(verts[face.verts[2]]);
                    vertices.Add(verts[face.verts[3]]);

                    if (type.color != null)
                    {
                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.zero);
                        uvs.Add(Vector2.zero);
                    }
                    else
                        AddTexture(type.GetTextureID(face), uvs);

                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);
                    colors.Add(color);

                    targetTriangles.Add(idx);
                    targetTriangles.Add(idx + 1);
                    targetTriangles.Add(idx + 2);
                    targetTriangles.Add(idx + 2);
                    targetTriangles.Add(idx + 1);
                    targetTriangles.Add(idx + 3);
                }
            }
        }

        private void CreateMesh(List<Vector3> vertices, List<int> triangles, List<int> coloredTriangles,
            List<Vector2> uvs, List<Color32> colors)
        {
            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                subMeshCount = 2,
                colors32 = colors.ToArray()
            };

            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(coloredTriangles, 1);

            mesh.RecalculateNormals();
            mesh.Optimize();

            Object.Destroy(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        void AddTexture(int textureID, List<Vector2> uvs)
        {
            float y = textureID / Voxels.TextureAtlasSizeInBlocks;
            float x = textureID - (y * Voxels.TextureAtlasSizeInBlocks);

            x *= Voxels.NormalizedBlockTextureSize;
            y *= Voxels.NormalizedBlockTextureSize;

            y = 1f - y - Voxels.NormalizedBlockTextureSize;
            float eps = 0.005f;
            uvs.Add(new Vector2(x + eps, y + eps));
            uvs.Add(new Vector2(x + eps, y + Voxels.NormalizedBlockTextureSize - eps));
            uvs.Add(new Vector2(x + Voxels.NormalizedBlockTextureSize - eps, y + eps));
            uvs.Add(new Vector2(x + Voxels.NormalizedBlockTextureSize - eps,
                y + Voxels.NormalizedBlockTextureSize - eps));
        }

        public void DeleteVoxel(VoxelPosition pos, Land land)
        {
            voxels[pos.local.x, pos.local.y, pos.local.z] = 0; //FIXME
            WorldService.INSTANCE.AddChange(pos, land);
            OnChanged(pos);
        }

        public void DeleteVoxels(Dictionary<VoxelPosition, Land> blocks)
        {
            var neighbourChunks = new HashSet<Chunk>();
            foreach (var pos in blocks.Keys)
            {
                var land = blocks[pos];
                voxels[pos.local.x, pos.local.y, pos.local.z] = 0;
                WorldService.INSTANCE.AddChange(pos, land);

                foreach (var neighborChunk in GetNeighborChunks(pos))
                    neighbourChunks.Add(neighborChunk);
            }

            DrawVoxels();
            foreach (var neighbor in neighbourChunks)
                neighbor.DrawVoxels();
        }

        public void PutVoxel(VoxelPosition pos, BlockType type, Land land)
        {
            voxels[pos.local.x, pos.local.y, pos.local.z] = type.id;
            WorldService.INSTANCE.AddChange(pos, type, land);
            OnChanged(pos);
        }

        public void PutVoxels(Dictionary<VoxelPosition, Tuple<BlockType, Land>> blocks)
        {
            var neighbourChunks = new HashSet<Chunk>();
            foreach (var pos in blocks.Keys)
            {
                var type = blocks[pos].Item1;
                var land = blocks[pos].Item2;
                voxels[pos.local.x, pos.local.y, pos.local.z] = type.id;
                WorldService.INSTANCE.AddChange(pos, type, land);
                foreach (var neighborChunk in GetNeighborChunks(pos))
                    neighbourChunks.Add(neighborChunk);
            }

            DrawVoxels();
            foreach (var neighbor in neighbourChunks)
                neighbor.DrawVoxels();
        }

        private IEnumerable<Chunk> GetNeighborChunks(VoxelPosition pos)
        {
            var neighborChunks = new HashSet<Chunk>();
            if (pos.local.x == voxels.GetLength(0) - 1)
                AddNeighborChunk(pos.chunk + Vector3Int.right, neighborChunks);

            if (pos.local.y == voxels.GetLength(1) - 1)
                AddNeighborChunk(pos.chunk + Vector3Int.up, neighborChunks);

            if (pos.local.z == voxels.GetLength(2) - 1)
                AddNeighborChunk(pos.chunk + Vector3Int.forward, neighborChunks);

            if (pos.local.x == 0)
                AddNeighborChunk(pos.chunk + Vector3Int.left, neighborChunks);

            if (pos.local.y == 0)
                AddNeighborChunk(pos.chunk + Vector3Int.down, neighborChunks);

            if (pos.local.z == 0)
                AddNeighborChunk(pos.chunk + Vector3Int.back, neighborChunks);

            return neighborChunks;
        }

        private void AddNeighborChunk(Vector3Int pos, ISet<Chunk> chunks)
        {
            var chunk = world.GetChunkIfInited(pos);
            if (chunk != null)
                chunks.Add(chunk);
        }

        public void PutMeta(VoxelPosition pos, MetaBlockType type, Land land)
        {
            metaBlocks = WorldService.INSTANCE.AddMetaBlock(pos, type, land);
            metaBlocks[pos.local].RenderAt(chunkObject.transform, pos.local, this);
        }

        public void DeleteMeta(VoxelPosition pos)
        {
            var block = metaBlocks[pos.local];
            metaBlocks.Remove(pos.local);
            WorldService.INSTANCE.OnMetaRemoved(block, new VoxelPosition(coordinate, pos.local).ToWorld()); // TODO ?
            block.Destroy();
        }

        private void OnChanged(VoxelPosition pos)
        {
            DrawVoxels();
            foreach (var neighborChunk in GetNeighborChunks(pos))
                neighborChunk.DrawVoxels();
        }

        private Vector3Int ToGlobal(Vector3Int localPoint)
        {
            return localPoint + new Vector3Int((int) position.x, (int) position.y, (int) position.z);
        }

        public bool IsActive()
        {
            return active;
        }

        public void SetActive(bool active)
        {
            if (chunkObject != null) chunkObject.SetActive(active);
            this.active = active;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;

            return coordinate.Equals(((Chunk) obj).coordinate);
        }

        public override int GetHashCode()
        {
            return coordinate.GetHashCode();
        }

        public void Destroy()
        {
            if (metaBlocks != null)
            {
                foreach (var metaBlock in metaBlocks.Values)
                    metaBlock.Destroy();
            }

            Object.Destroy(meshRenderer.sharedMaterials[1]);
            Object.Destroy(meshFilter.sharedMesh);
            Object.Destroy(chunkObject);
        }
    }
}