using System;
using System.Collections.Generic;
using Source.MetaBlocks;
using Source.Model;
using Source.Service;
using Source.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source
{
    public class Chunk
    {
        public readonly Vector3Int coordinate;

        public readonly Vector3Int position;
        private readonly uint[,,] voxels = new uint[CHUNK_SIZE.x, CHUNK_SIZE.y, CHUNK_SIZE.z];
        private bool active = true;
        public GameObject chunkObject;
        private bool inited;
        private bool initStarted;
        public MeshCollider meshCollider;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        private Dictionary<MetaLocalPosition, MetaBlock> metaBlocks;
        private readonly World world;

        public Chunk(Vector3Int coordinate, World world)
        {
            this.coordinate = coordinate;
            this.world = world;
            position = coordinate;
            position.Scale(CHUNK_SIZE);
        }

        /**
         * Written using expression so no one can change the properties
         */
        public static Vector3Int CHUNK_SIZE => new(16, 32, 16);

        public bool IsInitialized()
        {
            return initStarted;
        }

        public bool IsInitStarted()
        {
            return initStarted;
        }

        /**
         * returns false if initialization is already started.
         * If returned true, will call done when initialization ends.
         */
        public bool Init(Action done)
        {
            if (initStarted)
                return false;

            initStarted = true;
            chunkObject = new GameObject();
            chunkObject.SetActive(active);
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            meshCollider = chunkObject.AddComponent<MeshCollider>();
            var focusable = chunkObject.AddComponent<ChunkFocusable>();
            focusable.Initialize();

            meshRenderer.sharedMaterials = new[]
                {world.Material, new Material(Shader.Find("Particles/Standard Surface"))};
            chunkObject.transform.SetParent(world.transform);
            chunkObject.transform.position = position;
            chunkObject.name = "Chunk " + coordinate;

            ChunkInitializer.InitializeChunk(coordinate, voxels);
            WorldService.INSTANCE.GetChunkData(coordinate, data =>
            {
                if (data?.blocks != null)
                    foreach (var change in data.blocks)
                    {
                        var voxel = change.Key;
                        voxels[voxel.x, voxel.y, voxel.z] = change.Value;
                    }

                metaBlocks = data?.metaBlocks ?? new Dictionary<MetaLocalPosition, MetaBlock>();
                DrawVoxels();
                DrawMetaBlocks();
                inited = true;
                done.Invoke();
            });
            return true;
        }

        private void DrawMetaBlocks()
        {
            if (metaBlocks != null)
                foreach (var entry in metaBlocks)
                    entry.Value.RenderAt(chunkObject.transform, entry.Key.position, this);
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

        private void CreateMeshData(List<Vector3> vertices, List<int> triangles, List<int> coloredTriangles,
            List<Vector2> uvs,
            List<Color32> colors)
        {
            for (var y = 0; y < voxels.GetLength(1); y++)
            for (var x = 0; x < voxels.GetLength(0); x++)
            for (var z = 0; z < voxels.GetLength(2); z++)
                if (Blocks.GetBlockType(voxels[x, y, z]).isSolid)
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

            return Blocks.GetBlockType(voxels[localPos.x, localPos.y, localPos.z]);
        }

        public MetaBlock GetMetaAt(MetaPosition mp)
        {
            if (metaBlocks == null) return null;

            MetaBlock block;
            if (metaBlocks.TryGetValue(mp.local, out block))
                return block;

            return null;
        }

        private bool IsPositionSolidIfLoaded(Vector3Int localPos)
        {
            if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
                return world.IsSolidIfLoaded(ToGlobal(localPos));

            return GetBlock(localPos).isSolid;
        }

        private void AddVisibleFaces(Vector3Int pos, List<Vector3> vertices, List<int> triangles,
            List<int> coloredTriangles,
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
            var type = Blocks.GetBlockType(blockId);
            if (!type.isSolid) return;

            var targetTriangles = type.color != null ? coloredTriangles : triangles;
            var color = type.color ?? Color.white;

            foreach (var face in Voxels.Face.FACES)
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
                    {
                        AddTexture(type.GetTextureID(face), uvs);
                    }

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

        private void AddTexture(int textureID, List<Vector2> uvs)
        {
            float y = textureID / Voxels.TextureAtlasSizeInBlocks;
            var x = textureID - y * Voxels.TextureAtlasSizeInBlocks;

            x *= Voxels.NormalizedBlockTextureSize;
            y *= Voxels.NormalizedBlockTextureSize;

            y = 1f - y - Voxels.NormalizedBlockTextureSize;
            var eps = 0.005f;
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

        public void DeleteMeta(MetaPosition pos) // TODO [detach metablock]: add land param?
        {
            var block = metaBlocks[pos.local];
            metaBlocks.Remove(pos.local);
            WorldService.INSTANCE.OnMetaRemoved(block, new MetaPosition(coordinate, pos.local)); // TODO ?
            block.DestroyView();
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

        public void PutMeta(MetaPosition pos, MetaBlockType type, Land land)
        {
            var block = metaBlocks[pos.local] = WorldService.INSTANCE.AddMetaBlock(pos, type, land);
            block.RenderAt(chunkObject.transform, pos.local.position, this);
        }

        private void OnChanged(VoxelPosition pos)
        {
            DrawVoxels();
            foreach (var neighborChunk in GetNeighborChunks(pos))
                neighborChunk.DrawVoxels();
        }

        private Vector3Int ToGlobal(Vector3Int localPoint)
        {
            return localPoint + new Vector3Int(position.x, position.y, position.z);
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
            if (obj == null || GetType() != obj.GetType())
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
                foreach (var metaBlock in metaBlocks.Values)
                    metaBlock.DestroyView();

            Object.Destroy(meshRenderer.sharedMaterials[1]);
            Object.Destroy(meshFilter.sharedMesh);
            Object.Destroy(chunkObject);
        }
    }
}