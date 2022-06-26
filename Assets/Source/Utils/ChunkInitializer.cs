using System.Linq;
using Source.Canvas;
using Source.Model;
using Source.Service;
using Source.Ui.Login;
using Source.Ui.Menu;
using UnityEngine;

namespace Source.Utils
{
    public static class ChunkInitializer
    {
        private static readonly BlockType STONE = Blocks.GetBlockType("end_stone");
        private static readonly BlockType GRASS = Blocks.GetBlockType("grass");
        private static readonly BlockType DARK_GRASS = Blocks.GetBlockType("dark_grass");
        private static readonly BlockType DIRT = Blocks.GetBlockType("dirt");


        public static bool IsDefaultSolidAt(VoxelPosition vp)
        {
            return vp.chunk.y <= 0;
        }

        public static BlockType GetDefaultAt(VoxelPosition vp, bool hasLand, bool ownedByCurrentUser)
        {
            return vp.chunk.y > 0 ? Blocks.AIR :
                vp.chunk.y < 0 ? STONE :
                !hasLand ? STONE :
                vp.local.y < Chunk.CHUNK_SIZE.y - 1 ? DIRT :
                ownedByCurrentUser ? DARK_GRASS : GRASS;
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
                block = STONE.id;
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
            var wallet = Login.WalletId();

            Land land = null;
            var chunkSize = Chunk.CHUNK_SIZE;
            for (var x = 0; x < chunkSize.x; ++x)
            {
                for (var z = 0; z < chunkSize.z; ++z)
                {
                    top = body = STONE.id;
                    if (lands != null)
                    {
                        var pos = new Vector3Int(x + position.x * chunkSize.x, 0,
                            z + position.z * chunkSize.z);
                        if (land == null || !land.Contains(pos))
                            land = lands.FirstOrDefault(l => l.Contains(pos));
                        if (land != null)
                        {
                            body = DIRT.id;
                            top = land.owner.Equals(wallet) ? DARK_GRASS.id : GRASS.id;
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