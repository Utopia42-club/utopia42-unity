using System;
using Source.MetaBlocks;
using Source.MetaBlocks.VideoBlock;
using Source.Model;

namespace Source.Service.Migration.Models
{
    [Serializable]
    public class VideoBlockPropertiesLegacy : BaseImageBlockProperties<VideoFaceProps>
    {
    }

    [Serializable]
    public class VideoFaceProps : MetaBlockFaceProperties
    {
        public string url;
        public float previewTime = 0;

        public override BaseImageBlockProperties toProperties(SerializableVector3 rotation)
        {
            var video = new VideoBlockProperties()
            {
                url = url,
                previewTime = previewTime,
                height = height,
                width = width,
                detectCollision = detectCollision,
                rotation = rotation
            };
            return video;
        }
    }
}