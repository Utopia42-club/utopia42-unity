using Source.MetaBlocks.ImageBlock;
using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.VideoBlock
{
    public class VideoBlockType : MetaBlockType
    {
        public VideoBlockType(byte id) : base(id, "video", typeof(VideoBlockObject), typeof(VideoBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            VideoBlockObject.CreateVideoFace(World.INSTANCE.transform, MediaBlockEditor.DEFAULT_DIMENSION,
                    MediaBlockEditor.DEFAULT_DIMENSION, Vector3.zero, out var container, out _, out var renderer, false)
                .PlaceHolderInit(renderer, error);
            return container;
        }

        public override MetaPosition GetPlaceHolderPutPosition(Vector3 purePosition)
        {
            var pos = Player.INSTANCE.transform.forward.z > 0
                ? purePosition - ImageBlockType.Gap * Vector3.forward
                : purePosition + ImageBlockType.Gap * Vector3.forward;
            pos += 0.5f * MediaBlockEditor.DEFAULT_DIMENSION * Vector3.up;
            return new MetaPosition(pos);
        }
    }
}