using System;
using System.Collections.Generic;

namespace src.Model
{
    [Serializable]
    public class WorldSlice
    {
        public SerializableVector3Int startCoordinate;
        public SerializableVector3Int endCoordinate;

        /**
         * lands having intersections with this slice
         */
        public List<Land> lands;

        /**
         * chunk position string value ['x_y_z'] -> local [to chunk] position -> Block
         */
        public Dictionary<String, Dictionary<String, Block>> blocks;

        /**
         * chunk position string value ['x_y_z'] -> local [to chunk] position -> MetaBlock
         */
        public Dictionary<String, Dictionary<String, MetaBlockData>> metaBlocks;
    }
}