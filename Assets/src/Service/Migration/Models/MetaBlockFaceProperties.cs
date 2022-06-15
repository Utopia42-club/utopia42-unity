using System;
using src.MetaBlocks;
using src.Model;

namespace src.Service.Migration.Models
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