using System.Collections.Generic;
using Source.MetaBlocks;
using Source.Utils;
using UnityEngine;

namespace Source.Model
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
        public Dictionary<MetaLocalPosition, MetaBlock> metaBlocks;

        public ChunkData(Vector3Int position, Dictionary<Vector3Int, uint> blocks,
            Dictionary<MetaLocalPosition, MetaBlock> metaBlocks)
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

        public void ApplyChanges(ChunkData changes)
        {
            if (changes.blocks != null)
            {
                blocks ??= new Dictionary<Vector3Int, uint>();
                foreach (var entry in changes.blocks)
                    blocks[entry.Key] = entry.Value;
            }

            if (changes.metaBlocks != null)
            {
                metaBlocks ??= new Dictionary<MetaLocalPosition, MetaBlock>();
                foreach (var entry in changes.metaBlocks)
                {
                    if (entry.Value == MetaBlock.DELETED_METABLOCK)
                        metaBlocks.Remove(entry.Key);
                    else
                        metaBlocks[entry.Key] = entry.Value;
                }
            }
        }


        public ChunkData Clone()
        {
            var clone = new ChunkData(position, null, null);
            clone.ApplyChanges(this);
            return clone;
        }
    }
}