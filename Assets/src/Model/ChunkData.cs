using System.Collections.Generic;
using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src.Model
{
    public class ChunkData
    {
        public Vector3Int position;

        /**
         * local [to chunk] position -> Block id
         */
        public Dictionary<Vector3Int, uint> blocks;

        /**
         * local [to chunk] position -> MetaBlock
         */
        public Dictionary<Vector3Int, MetaBlock> metaBlocks;

        public ChunkData(Vector3Int position, Dictionary<Vector3Int, uint> blocks,
            Dictionary<Vector3Int, MetaBlock> metaBlocks)
        {
            this.position = position;
            this.blocks = blocks;
            this.metaBlocks = metaBlocks;
        }

        public BlockType GetBlockTypeAt(Vector3Int localPosition)
        {
            return blocks != null && blocks.TryGetValue(localPosition, out var typeId)
                ? Blocks.GetBlockType(typeId)
                : null;
        }

        public void AddAll(ChunkData data)
        {
            if (data.blocks != null)
            {
                blocks ??= new Dictionary<Vector3Int, uint>();
                foreach (var entry in data.blocks)
                    blocks.Add(entry.Key, entry.Value);
            }

            if (data.metaBlocks != null)
            {
                metaBlocks ??= new Dictionary<Vector3Int, MetaBlock>();
                foreach (var entry in data.metaBlocks)
                    metaBlocks.Add(entry.Key, entry.Value);
            }
        }


        public ChunkData Clone()
        {
            var clone = new ChunkData(position, null, null);
            clone.AddAll(this);
            return clone;
        }
    }
}