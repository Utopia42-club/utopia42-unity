using System;
using src.MetaBlocks;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src
{
    public class HighlightedMetaBlock : MonoBehaviour
    {
        public MetaBlockType type { get; private set; }
        public uint MetaBlockTypeId => type.id;
        public object MetaProperties { get; private set; }
        
        private GameObject metaBlockHighlight;
        private Vector3 metaBlockHighlightPosition;
        private GameObject referenceGo;
        public Vector3 Offset { get; private set; } = Vector3.zero; // might change because of rotation only
        public MetaLocalPosition Position { get; private set; }
        public MetaLocalPosition CurrentPosition => new MetaLocalPosition(Position.position + Offset);

        public static HighlightedMetaBlock Create(MetaLocalPosition localPos, HighlightChunk highlightChunk, MetaBlock meta)
        {
            
            var highlightedBlock = highlightChunk.gameObject.AddComponent<HighlightedMetaBlock>();
            highlightedBlock.Position = localPos;
            highlightedBlock.transform.SetParent(highlightChunk.transform);
            highlightedBlock.type = meta.type;
            highlightedBlock.MetaProperties = ((ICloneable) meta.GetProps())?.Clone();
        
            meta.CreateSelectHighlight(highlightChunk.transform, localPos, highlight =>
            {
                if (highlightedBlock.metaBlockHighlight != null)
                {
                    DestroyImmediate(highlightedBlock.metaBlockHighlight);
                    highlightedBlock.metaBlockHighlight = null;
                }
        
                highlightedBlock.metaBlockHighlight = highlight;
                highlightedBlock.metaBlockHighlightPosition =
                    highlight.transform.localPosition + World.INSTANCE.MetaHighlightOffset; // TODO ?
                highlightedBlock.UpdateMetaBlockHighlightPosition();
            }, out highlightedBlock.referenceGo);
        
            return highlightedBlock;
        }

        public void Rotate(Vector3 center, Vector3 axis, Vector3Int chunkPosition)
        {
            var currentPosition = Vectors.TruncateFloor(CurrentPosition.position) + chunkPosition + World.INSTANCE.HighlightOffset;
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