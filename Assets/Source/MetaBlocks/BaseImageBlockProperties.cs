using System;
using Source.Model;

namespace Source.MetaBlocks
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