using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class ChunkFocusable : Focusable
    {
        private Chunk chunk;
        public void Initialize(Chunk chunk)
        {
            if(initialized) return;
            this.chunk = chunk;
            initialized = true;
        }

        public override void Focus(Vector3? point = null)
        {
            if(!initialized || point == null) return;
            Player.INSTANCE.PlaceCursorBlocks(point.Value, chunk);
        }

        public override void UnFocus()
        {
        }

        public override Vector3 GetBlockPosition()
        {
            return Player.INSTANCE.HighlightBlock.position;
        }
    }
}