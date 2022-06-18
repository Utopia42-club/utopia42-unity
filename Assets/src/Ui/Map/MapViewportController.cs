using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.Ui.Map
{
    internal class MapViewportController
    {
        private readonly Map map;
        private readonly Action<ViewportChangeEvent> listener;
        private bool dragging = false;
        private float scale = 1;
        private Rect rect;
        private Vector2 dragPrevPosition;

        public MapViewportController(Map map, Action<ViewportChangeEvent> listener)
        {
            this.map = map;
            this.listener = listener;
            this.map.RegisterCallback<PointerMoveEvent>(PointerMoved);
            this.map.RegisterCallback<MouseDownEvent>(e =>
            {
                if (!e.ctrlKey)
                {
                    e.StopPropagation();
                    dragPrevPosition = e.mousePosition;
                    this.map.CaptureMouse();
                    dragging = true;
                }
            });
            this.map.RegisterCallback<MouseUpEvent>(e =>
            {
                if (!this.map.HasMouseCapture() || !dragging) return;
                dragging = false;
                e.StopPropagation();
                this.map.ReleaseMouse();
            });
            this.map.RegisterCallback<GeometryChangedEvent>(e => UpdateSize(e.newRect.width, e.newRect.height));
            this.map.RegisterCallback<WheelEvent>(e => Scale(e));
        }

        private void Scale(WheelEvent e)
        {
            if (e.delta.y == 0) return;

            var multiplier = e.delta.y > 0 ? (float) 0.5 : 2;
            scale = Mathf.Clamp(scale * multiplier, 0.25f, 4f);
            var mouseBeforeScale = map.ScreenToUtopia(e.mousePosition);
            listener(new ViewportChangeEvent(rect, scale));
            // Change the viewport inorder to maintain local (utopia position) mouse position while scaling 
            var post = map.UtopiaToScreen(mouseBeforeScale);
            var delta = e.mousePosition - post;
            rect = new Rect(rect.x - delta.x, rect.y - delta.y, rect.width, rect.height);
            listener(new ViewportChangeEvent(rect, scale));
        }

        private void UpdateSize(float width, float height)
        {
            rect = new Rect(rect.x, rect.y, width, height);
            listener(new ViewportChangeEvent(rect, scale));
        }

        private void PointerMoved(PointerMoveEvent e)
        {
            if (map.HasMouseCapture())
            {
                e.StopPropagation();
                var delta = (Vector2) e.position - dragPrevPosition;
                dragPrevPosition = e.position;
                rect = new Rect(rect.x - delta.x, rect.y - delta.y, rect.width, rect.height);
                listener(new ViewportChangeEvent(rect, scale));
            }
        }
    }

    internal class ViewportChangeEvent
    {
        public readonly Rect rect;
        public readonly float scale;

        public ViewportChangeEvent(Rect rect, float scale)
        {
            this.rect = rect;
            this.scale = scale;
        }
    }
}