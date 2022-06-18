using System.Collections.Generic;
using UnityEngine;

namespace Source.Canvas.Map
{
    public class MapGrid : MonoBehaviour
    {
        private readonly GridLines vLines;
        private readonly GridLines hLines;

        public MapGrid()
        {
            vLines = new GridLines(true, this);
            hLines = new GridLines(false, this);
        }

        private void Start()
        {

        }

        private void Update()
        {
            Span();
        }

        private void Span()
        {
            var center = -1 * transform.parent.localPosition;

            var left = center.x - Screen.width / 2;
            var right = center.x + Screen.width / 2;
            var bottom = center.y - Screen.height / 2;
            var top = center.y + Screen.height / 2;

            vLines.Span(left, right, bottom);
            hLines.Span(bottom, top, left);
        }


        private class GridLines
        {
            readonly bool vertical;
            readonly MapGrid grid;
            readonly Queue<GridLine> invisible = new Queue<GridLine>();
            readonly LinkedList<GridLine> visible = new LinkedList<GridLine>();

            public GridLines(bool vertical, MapGrid grid)
            {
                this.vertical = vertical;
                this.grid = grid;
            }

            internal void Span(float from, float to, float center)
            {
                var fromIdx = Mathf.FloorToInt(from / GridLine.SPACE);
                var toIdx = Mathf.FloorToInt(to / GridLine.SPACE);
                Trim(fromIdx, toIdx);

                if (visible.Count == 0) visible.AddLast(DeqOrCreate(toIdx));

                while (visible.First.Value.GetIndex() > fromIdx)
                    visible.AddFirst(DeqOrCreate(visible.First.Value.GetIndex() - 1));

                while (visible.Last.Value.GetIndex() < toIdx)
                    visible.AddLast(DeqOrCreate(visible.Last.Value.GetIndex() + 1));

                foreach (var line in visible)
                    line.SetPos(center);
            }

            private void Trim(int fromIdx, int toIdx)
            {
                while (visible.Count > 0 && visible.First.Value.GetIndex() < fromIdx)
                {
                    invisible.Enqueue(visible.First.Value);
                    visible.RemoveFirst();
                }
                while (visible.Count > 0 && visible.Last.Value.GetIndex() > toIdx)
                {
                    invisible.Enqueue(visible.Last.Value);
                    visible.RemoveLast();
                }
            }

            private GridLine DeqOrCreate(int index)
            {
                if (invisible.Count != 0)
                {
                    var line = invisible.Dequeue();
                    line.SetIndex(index);
                    return line;
                }
                return GridLine.create(grid.transform, vertical, index);
            }
        }
    }
}
