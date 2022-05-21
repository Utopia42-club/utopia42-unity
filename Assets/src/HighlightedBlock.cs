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
        private Vector3 metaBlockHighlightPosition; // temp

        public Vector3Int Offset { get; private set; } // might change because of rotation only
        public Vector3Int Position { get; private set; } // original local position to highlight chunk
        public Vector3Int CurrentPosition => Position + Offset;

        public static HighlightedBlock Create(Vector3Int localPos, Vector3Int offset, HighlightChunk highlightChunk,
            uint blockTypeId,
            MetaBlock meta = null)
        {
            var highlightedBlock = highlightChunk.gameObject.AddComponent<HighlightedBlock>();
            highlightedBlock.Position = localPos;
            highlightedBlock.Offset = offset;
            highlightedBlock.BlockTypeId = blockTypeId;
            highlightedBlock.transform.SetParent(highlightChunk.transform);

            if (meta == null) return highlightedBlock;

            var metaBlockHighlight = meta.blockObject.CreateSelectHighlight()?.gameObject; // TODO: if the highlight original chunk is inactive then we cannot see the highlight (for instance after pasting)
            highlightedBlock.MetaAttached = true;
            highlightedBlock.MetaBlockTypeId = meta.type.id;
            highlightedBlock.MetaProperties = ((ICloneable) meta.GetProps())?.Clone();
            highlightedBlock.metaBlockHighlight =
                metaBlockHighlight; // TODO: set parent so that it would be destroyed with its SelectedBlock (custom OnDestroy would be removed)
            if (metaBlockHighlight != null)
                highlightedBlock.metaBlockHighlightPosition = metaBlockHighlight.transform.position;
            return highlightedBlock;
        }

        public void Rotate(Vector3 center, Vector3 axis) // Reset offset and move metaHighlight accordingly
        {
            var currentPosition = CurrentPosition;
            var rotatedVector = Quaternion.AngleAxis(90, axis) *
                                (currentPosition + 0.5f * Vector3.one - center);
            var newPosition = Vectors.TruncateFloor(center + rotatedVector - 0.5f * Vector3.one);
            Offset = Offset + newPosition - currentPosition;
        }

        public void UpdateMetaBlockHighlightPosition() // TODO: modify?
        {
            if (metaBlockHighlight == null) return;
                metaBlockHighlight.transform.position = metaBlockHighlightPosition + Offset + World.INSTANCE.HighlightOffset;
        }

        private void OnDestroy()
        {
            if (metaBlockHighlight != null)
                DestroyImmediate(metaBlockHighlight);
        }
    }
}