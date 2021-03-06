using Source.Model;
using UnityEngine;

namespace Source
{
    public class ChunkFocusable : Focusable
    {
        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
        }

        public override void Focus(Vector3? point = null)
        {
            if (!initialized || !point.HasValue) return;
            Player.INSTANCE.PlaceCursors(point.Value);
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