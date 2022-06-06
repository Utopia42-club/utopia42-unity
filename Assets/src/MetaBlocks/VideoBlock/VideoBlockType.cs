using src.Model;
using UnityEngine;

namespace src.MetaBlocks.VideoBlock
{
    public class VideoBlockType : MetaBlockType
    {
        public VideoBlockType(byte id) : base(id, "video", typeof(VideoBlockObject), typeof(VideoBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            VideoBlockObject.CreateVideoFace(World.INSTANCE.transform, VideoBlockEditor.DEFAULT_DIMENSION,
                    VideoBlockEditor.DEFAULT_DIMENSION, Vector3.zero, out var container, out _, out var renderer, false)
                .PlaceHolderInit(renderer, error);
            return container;
        }
        
        public override MetaPosition GetPutPosition(Vector3 purePosition, Vector3 playerForward)
        {
            var pos = playerForward.z > 0
                ? purePosition - 0.2f * Vector3.forward
                : purePosition + 0.2f * Vector3.forward;
            pos += 0.5f * VideoBlockEditor.DEFAULT_DIMENSION * Vector3.up;
            return new MetaPosition(pos);
        }
    }
}