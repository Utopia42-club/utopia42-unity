using System;
using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class HighlightedBlock : MonoBehaviour
    {
        public uint BlockTypeId { get; private set; }
        public uint MetaBlockTypeId { get; private set; }
        public object MetaProperties { get; private set; }
        public bool MetaAttached { get; private set; } = false;
        private GameObject metaBlockHighlight;
        private Vector3 metaBlockHighlightPosition;
        private GameObject referenceGo;

        public Vector3Int Offset { get; private set; } = Vector3Int.zero; // might change because of rotation only
        public Vector3Int Position { get; private set; } // original local position to highlight chunk
        public Vector3Int CurrentPosition => Position + Offset;

        public static HighlightedBlock Create(Vector3Int localPos, HighlightChunk highlightChunk,
            uint blockTypeId,
            MetaBlock meta = null)
        {
            var highlightedBlock = highlightChunk.gameObject.AddComponent<HighlightedBlock>();
            highlightedBlock.Position = localPos;
            highlightedBlock.BlockTypeId = blockTypeId;
            highlightedBlock.transform.SetParent(highlightChunk.transform);
            if (meta == null) return highlightedBlock;

            highlightedBlock.MetaAttached = true;
            highlightedBlock.MetaBlockTypeId = meta.type.id;
            highlightedBlock.MetaProperties = ((ICloneable) meta.GetProps())?.Clone();

            meta.CreateSelectHighlight(highlightChunk.transform, localPos, highlight =>
            {
                highlightedBlock.metaBlockHighlight = highlight;
                highlightedBlock.metaBlockHighlightPosition = highlight.transform.localPosition + World.INSTANCE.HighlightOffset; // TODO ?
                highlightedBlock.UpdateMetaBlockHighlightPosition();
            }, out highlightedBlock.referenceGo);

            return highlightedBlock;
        }

        public void Rotate(Vector3 center, Vector3 axis, Vector3Int chunkPosition)
        {
            var currentPosition = CurrentPosition + chunkPosition + World.INSTANCE.HighlightOffset;
            var rotatedVector = Quaternion.AngleAxis(90, axis) *
                                (currentPosition + 0.5f * Vector3.one - center);
            var newPosition = Vectors.TruncateFloor(center + rotatedVector - 0.5f * Vector3.one);
            Offset = Offset + newPosition - currentPosition;
        }

        public void UpdateMetaBlockHighlightPosition()
        {
            if (metaBlockHighlight == null) return;
            metaBlockHighlight.transform.localPosition = metaBlockHighlightPosition + Offset;
        }

        private void OnDestroy()
        {
            if (metaBlockHighlight != null)
                DestroyImmediate(metaBlockHighlight);
            if (referenceGo != null)
                DestroyImmediate(referenceGo);
        }
    }
}