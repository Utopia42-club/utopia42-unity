using System.Linq;
using src.Canvas;
using src.Model;
using src.Service;
using UnityEngine;

namespace src.Utils
{
    public static class ChunkInitializer
    {
        private static readonly uint STONE = Blocks.GetBlockType("end_stone").id;
        private static readonly uint GRASS = Blocks.GetBlockType("grass").id;
        private static readonly uint DARK_GRASS = Blocks.GetBlockType("dark_grass").id;
        private static readonly uint DIRT = Blocks.GetBlockType("dirt").id;


        public static bool IsDefaultSolidAt(VoxelPosition vp)
        {
            return vp.chunk.y <= 0;
        }
        
        public static void InitializeChunk(Vector3Int position, uint[,,] voxels)
        {
            if (position.y == 0)
            {
                InitGroundLevel(position, voxels);
                return;
            }

            uint block;
            if (position.y < 0)
                block = STONE;
            else
                block = Blocks.AIR.id;

            for (int x = 0; x < voxels.GetLength(0); x++)
            {
                for (int z = 0; z < voxels.GetLength(2); z++)
                    FillAtXY(block, block, x, z, voxels);
            }
        }

        private static void InitGroundLevel(Vector3Int position, uint[,,] voxels)
        {
            uint body, top;
            var worldService = WorldService.INSTANCE;

            var lands = worldService.GetLandsForChunk(new Vector2Int(position.x, position.z));
            var wallet = Settings.WalletId();

            Land land = null;
            var chunkSize = Chunk.CHUNK_SIZE;
            for (var x = 0; x < chunkSize.x; ++x)
            {
                for (var z = 0; z < chunkSize.z; ++z)
                {
                    top = body = STONE;
                    if (lands != null)
                    {
                        var pos = new Vector3Int(x + position.x * chunkSize.x, 0,
                            z + position.z * chunkSize.z);
                        if (land == null || !land.Contains(pos))
                            land = lands.FirstOrDefault(l => l.Contains(pos));
                        if (land != null)
                        {
                            body = DIRT;
                            top = land.owner.Equals(wallet) ? DARK_GRASS : GRASS;
                        }
                    }

                    FillAtXY(top, body, x, z, voxels);
                }
            }
        }

        private static void FillAtXY(uint top, uint body, int x, int z, uint[,,] voxels)
        {
            int maxy = voxels.GetLength(1) - 1;

            voxels[x, maxy, z] = top;
            for (int y = 0; y < maxy; y++)
                voxels[x, y, z] = body;
        }
    }
}