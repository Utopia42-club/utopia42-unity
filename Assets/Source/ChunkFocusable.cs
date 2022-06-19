using Source.MetaBlocks;
using Source.Model;
using Source.Utils;
using UnityEngine;

namespace Source
{
    public class ChunkFocusable : Focusable
    {
        private Chunk chunk;

        public void Initialize(Chunk chunk)
        {
            if (initialized) return;
            this.chunk = chunk;
            initialized = true;
        }

        public override void Focus(Vector3? point = null)
        {
            if (!initialized || point == null) return;
            Player.INSTANCE.PlaceCursorBlocks(point.Value, chunk);
        }

        public override void UnFocus()
        {
        }

        public override Vector3? GetBlockPosition()
        {
            if (!Player.INSTANCE.HighlightBlock.gameObject.activeSelf) return null;
            return Player.INSTANCE.PossibleHighlightBlockPosInt;
        }

        public override bool IsSelected()
        {
            return World.INSTANCE.IsSelected(new VoxelPosition(Player.INSTANCE.PossibleHighlightBlockPosInt));
        }
    }
}