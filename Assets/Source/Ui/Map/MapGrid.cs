using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapGrid : VisualElement
    {
        private readonly Subgrid verticalSubgrid;
        private readonly Subgrid horizontalSubgrid;

        public MapGrid()
        {
            AddToClassList("map-grid");
            Add(horizontalSubgrid = new Subgrid(false));
            Add(verticalSubgrid = new Subgrid(true));
        }

        internal void UpdateViewport(ViewportChangeEvent e)
        {
            verticalSubgrid.style.left = -e.rect.x;
            verticalSubgrid.Span(e.rect.xMin, e.rect.xMax);
            horizontalSubgrid.style.top = -e.rect.y;
            horizontalSubgrid.Span(e.rect.yMin, e.rect.yMax);
        }

        private class Subgrid : VisualElement
        {
            /**
             * Lines out of screen bounds
             */
            private readonly Queue<Line> invisibleLines = new();

            /**
             * Lines inside of screen bounds
             */
            private readonly LinkedList<Line> visibleLines = new();

            public readonly bool vertical;

            public Subgrid(bool vertical)
            {
                this.vertical = vertical;
                AddToClassList("map-subgrid");
            }


            internal void Span(float from, float to)
            {
                var fromIdx = Mathf.FloorToInt(from / Line.TickSize);
                var toIdx = Mathf.FloorToInt(to / Line.TickSize);
                Trim(fromIdx, toIdx);

                if (visibleLines.Count == 0) visibleLines.AddLast(DeqOrCreate(toIdx));

                while (visibleLines.First.Value.GetIndex() > fromIdx)
                    visibleLines.AddFirst(DeqOrCreate(visibleLines.First.Value.GetIndex() - 1));

                while (visibleLines.Last.Value.GetIndex() < toIdx)
                    visibleLines.AddLast(DeqOrCreate(visibleLines.Last.Value.GetIndex() + 1));
            }

            private void Trim(int fromIdx, int toIdx)
            {
                while (visibleLines.Count > 0 && visibleLines.First.Value.GetIndex() < fromIdx)
                {
                    invisibleLines.Enqueue(visibleLines.First.Value);
                    visibleLines.RemoveFirst();
                }

                while (visibleLines.Count > 0 && visibleLines.Last.Value.GetIndex() > toIdx)
                {
                    invisibleLines.Enqueue(visibleLines.Last.Value);
                    visibleLines.RemoveLast();
                }
            }

            private Line DeqOrCreate(int index)
            {
                if (invisibleLines.Count != 0)
                {
                    var line = invisibleLines.Dequeue();
                    line.SetIndex(index);
                    return line;
                }

                var l = new Line(this, index);
                Add(l);
                return l;
            }
        }

        private class Line : VisualElement
        {
            internal const float TickSize = 50;
            private readonly Subgrid subgrid;
            private int index;

            public Line(Subgrid subgrid, int index)
            {
                this.subgrid = subgrid;
                AddToClassList(subgrid.vertical ? "map-vertical-grid-line" : "map-horizontal-grid-line");
                const int minorTickSize = (int) TickSize / 5;
                for (int i = 1; i < 6; i++)
                {
                    var minor = new VisualElement();
                    minor.AddToClassList("minor-grid-line");
                    if (subgrid.vertical)
                        minor.style.left = minorTickSize * i;
                    else minor.style.top = minorTickSize * i;
                    Add(minor);
                }

                SetIndex(index);
            }

            public void SetIndex(int index)
            {
                this.index = index;
                if (subgrid.vertical)
                    style.left = TickSize * index;
                else style.top = TickSize * index;
            }

            internal int GetIndex()
            {
                return this.index;
            }
        }
    }
}