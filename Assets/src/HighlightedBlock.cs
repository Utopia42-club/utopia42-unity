using System;
using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class HighlightedBlock : MonoBehaviour
    {
        public uint BlockTypeId { get; private set; }

        public Vector3Int Offset { get; private set; } = Vector3Int.zero; // might change because of rotation only
        public Vector3Int Position { get; private set; } // original local position to highlight chunk
        public Vector3Int CurrentPosition => Position + Offset;

        public static HighlightedBlock Create(Vector3Int localPos, HighlightChunk highlightChunk, uint blockTypeId)
        {
            var highlightedBlock = highlightChunk.gameObject.AddComponent<HighlightedBlock>();
            highlightedBlock.Position = localPos;
            highlightedBlock.BlockTypeId = blockTypeId;
            highlightedBlock.transform.SetParent(highlightChunk.transform);
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
    }
}