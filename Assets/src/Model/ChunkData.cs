using System.Collections.Generic;
using src.MetaBlocks;
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

        public ChunkData(Vector3Int position, Dictionary<Vector3Int, uint> blocks, Dictionary<Vector3Int, MetaBlock> metaBlocks)
        {
            this.position = position;
            this.blocks = blocks;
            this.metaBlocks = metaBlocks;
        }
    }
}