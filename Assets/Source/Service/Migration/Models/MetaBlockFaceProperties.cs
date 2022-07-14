using System;
using Source.MetaBlocks;
using Source.Model;

namespace Source.Service.Migration.Models
{
    [Serializable]
    public abstract class MetaBlockFaceProperties
    {
        public int width;
        public int height;
        public bool detectCollision = true;

        public abstract BaseImageBlockProperties toProperties(SerializableVector3 rotation);
    }
}