using System;
using Source.MetaBlocks;
using Source.Model;
using UnityEngine;

namespace Source
{
    public class HighlightedMetaBlock : MonoBehaviour
    {
        private Action cleanUpAction;

        private GameObject metaBlockHighlight;
        private Vector3 metaBlockHighlightPosition;

        private Transform rotationTransform;
        private Transform scaleTransform;
        public MetaBlockType type { get; private set; }
        public uint MetaBlockTypeId => type.id;
        public object MetaProperties { get; private set; }

        public Vector3 Offset { get; } = Vector3.zero; // might change because of rotation only
        public MetaLocalPosition Position { get; private set; }
        public MetaLocalPosition CurrentPosition => new(Position.position + Offset);

        private void OnDestroy()
        {
            cleanUpAction?.Invoke();
            if (metaBlockHighlight != null)
                DestroyImmediate(metaBlockHighlight);
        }

        public static void CreateAndAddToChunk(MetaLocalPosition localPos, HighlightChunk highlightChunk,
            MetaBlock meta, Action<HighlightedMetaBlock> onLoad = null)
        {
            if (highlightChunk == null) return; // TODO ?
            var highlightedBlock = highlightChunk.gameObject.AddComponent<HighlightedMetaBlock>();
            highlightedBlock.Position = localPos;
            highlightedBlock.transform.SetParent(highlightChunk.transform);
            highlightedBlock.type = meta.type;
            highlightedBlock.MetaProperties = ((ICloneable) meta.GetProps())?.Clone();

            meta.CreateSelectHighlight(highlightChunk.transform, localPos, highlight =>
            {
                onLoad?.Invoke(highlightedBlock);
                onLoad = null;
                if (highlightedBlock.metaBlockHighlight != null)
                {
                    DestroyImmediate(highlightedBlock.metaBlockHighlight);
                    highlightedBlock.metaBlockHighlight = null;
                }

                highlightedBlock.metaBlockHighlight = highlight;
                highlightedBlock.metaBlockHighlightPosition =
                    highlight.transform.localPosition + World.INSTANCE.MetaHighlightOffset; // TODO ?
                highlightedBlock.UpdateMetaBlockHighlightPosition();

                // reset scale-rotation targets
                World.INSTANCE.ObjectScaleRotationController.Detach(highlightedBlock.scaleTransform,
                    highlightedBlock.rotationTransform);

                if (meta.blockObject == null) return;

                var scaleTransform = meta.blockObject.GetScaleTarget(out var afterScaled);
                if (scaleTransform != null)
                {
                    World.INSTANCE.ObjectScaleRotationController.AttachScaleTarget(scaleTransform, afterScaled);
                    highlightedBlock.scaleTransform = scaleTransform;
                }

                var rotationTransform = meta.blockObject.GetRotationTarget(out var afterRotated);
                if (rotationTransform != null)
                {
                    World.INSTANCE.ObjectScaleRotationController.AttachRotationTarget(rotationTransform, afterRotated);
                    highlightedBlock.rotationTransform = rotationTransform;
                }

                // reset properties (it may have been changed due to rotation or scaling)
                highlightedBlock.MetaProperties = ((ICloneable) meta.GetProps())?.Clone();
            }, out highlightedBlock.cleanUpAction);
        }

        // public void Rotate(Vector3 center, Vector3 axis, Vector3Int chunkPosition)
        // {
        //     var currentPosition = Vectors.TruncateFloor(CurrentPosition.position) + chunkPosition + World.INSTANCE.HighlightOffset;
        //     var rotatedVector = Quaternion.AngleAxis(90, axis) *
        //                         (currentPosition + 0.5f * Vector3.one - center);
        //     var newPosition = Vectors.TruncateFloor(center + rotatedVector - 0.5f * Vector3.one);
        //     Offset = Offset + newPosition - currentPosition;
        // }

        public void UpdateMetaBlockHighlightPosition()
        {
            if (metaBlockHighlight == null) return;
            metaBlockHighlight.transform.localPosition = metaBlockHighlightPosition + Offset;
        }
    }
}