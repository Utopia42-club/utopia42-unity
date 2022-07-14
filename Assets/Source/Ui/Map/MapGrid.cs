using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapGrid : VisualElement
    {
        private readonly Map map;
        private readonly Subgrid verticalSubgrid;
        private readonly Subgrid horizontalSubgrid;

        public MapGrid(Map map)
        {
            this.map = map;
            AddToClassList("map-grid");
            Add(horizontalSubgrid = new Subgrid(false));
            Add(verticalSubgrid = new Subgrid(true));
        }

        internal void UpdateViewport(float scale)
        {
            var origin = this.WorldToLocal(map.UtopiaToScreen(Vector2.zero));
            verticalSubgrid.Span(origin.x, contentRect.width, scale);
            horizontalSubgrid.Span(origin.y, contentRect.height, scale);
        }

        private class Subgrid : VisualElement
        {
            private const float BaseTickSize = 50;
            public readonly bool vertical;

            public Subgrid(bool vertical)
            {
                this.vertical = vertical;
                AddToClassList("map-subgrid");
            }


            internal void Span(float origin, float size, float scale)
            {
                Clear();
                var tick = (scale < 0.5f ? BaseTickSize * 2 :
                    scale > 2f ? BaseTickSize / 2 :
                    BaseTickSize) * scale;
                var first = Mathf.Ceil((-origin) / tick) * tick + origin - tick;
                var count = Mathf.Ceil(size / tick) + 1;
                for (var i = 0; i < count; i++)
                {
                    var pos = first + i * tick;
                    Add(new Line(this, pos, Math.Abs(pos - origin) < 1e-5));
                    if (tick >= 50)
                    {
                        var mtick = tick / 5;
                        for (var j = 1; j < 5; j++)
                        {
                            var l = new Line(this, pos + j * mtick, false);
                            l.AddToClassList("minor-grid-line");
                            Add(l);
                        }
                    }
                }
            }
        }

        private class Line : VisualElement
        {
            public Line(Subgrid subgrid, float position, bool isOrigin)
            {
                AddToClassList(subgrid.vertical ? "map-vertical-grid-line" : "map-horizontal-grid-line");
                if (subgrid.vertical)
                    style.left = position;
                else
                    style.top = position;
                if (isOrigin)
                    AddToClassList("map-origin-grid-line");
            }
        }
    }
}