using System;
using Source.MetaBlocks;
using Source.MetaBlocks.ImageBlock;
using Source.Model;

namespace Source.Service.Migration.Models
{
    [Serializable]
    public class MediaBlockPropertiesLegacy : BaseImageBlockProperties<MediaFaceProps>
    {
    }

    [Serializable]
    public class MediaFaceProps: MetaBlockFaceProperties
    {
        public string url;
        public override BaseImageBlockProperties toProperties(SerializableVector3 rotation)
        {
            var image = new MediaBlockProperties
            {
                url = url,
                height = height,
                width = width,
                detectCollision = detectCollision,
                rotation = rotation
            };
            return image;
        }
    }
}