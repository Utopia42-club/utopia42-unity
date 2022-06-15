using System;
using src.Model;

namespace src.MetaBlocks
{
    [Serializable]
    public class BaseImageBlockProperties
    {
        public int width;
        public int height;
        public bool detectCollision = true;
        public SerializableVector3 rotation = SerializableVector3.Zero;
    }
}