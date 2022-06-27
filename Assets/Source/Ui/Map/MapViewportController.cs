using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    internal class MapViewportController
    {
        private const int defaultZoomIndex = 7;

        private readonly float[] scales = new[]
        {
            0.25f, 0.33f, 0.50f, 0.67f, 0.75f, 0.8f, 0.9f, 1,
            1.1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f, 4f, 5f
        };

        private readonly Map map;
        private readonly Action<ViewportChangeEvent> listener;
        private bool dragging = false;
        private int scaleIndex = defaultZoomIndex;
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
            this.map.RegisterCallback<WheelEvent>(OnMouseWheelEvent);
        }

        private void OnMouseWheelEvent(WheelEvent e)
        {
            if (e.delta.y == 0) return;
            Scale(e.mousePosition, e.delta.y <= 0);
        }

        public void ZoomIn()
        {
            Scale(new Vector2(Screen.width / 2, Screen.height / 2), true);
        }

        public void ZoomOut()
        {
            Scale(new Vector2(Screen.width / 2, Screen.height / 2), false);
        }

        public void BackToDefaultZoom()
        {
            DoScale(new Vector2(Screen.width / 2, Screen.height / 2), defaultZoomIndex);
        }

        private void Scale(Vector2 mousePosition, bool zoomIn)
        {
            var additive = zoomIn ? 1 : -1;
            var newIdx = scaleIndex + additive;

            DoScale(mousePosition, newIdx);
        }

        private void DoScale(Vector2 mousePosition, int newIdx)
        {
            if (newIdx >= 0 && newIdx < scales.Length)
            {
                scaleIndex = newIdx;
                var scale = scales[scaleIndex];
                var mouseBeforeScale = map.ScreenToUtopia(mousePosition);
                listener(new ViewportChangeEvent(rect, scale));
                // Change the viewport inorder to maintain local (utopia position) mouse position while scaling 
                var post = map.UtopiaToScreen(mouseBeforeScale);
                var delta = mousePosition - post;
                rect = new Rect(rect.x - delta.x, rect.y - delta.y, rect.width, rect.height);
                listener(new ViewportChangeEvent(rect, scale));
            }
        }

        private void UpdateSize(float width, float height)
        {
            rect = new Rect(rect.x, rect.y, width, height);
            listener(new ViewportChangeEvent(rect, scales[scaleIndex]));
        }

        private void PointerMoved(PointerMoveEvent e)
        {
            if (map.HasMouseCapture() && dragging)
            {
                e.StopPropagation();
                var delta = (Vector2) e.position - dragPrevPosition;
                dragPrevPosition = e.position;
                rect = new Rect(rect.x - delta.x, rect.y - delta.y, rect.width, rect.height);
                listener(new ViewportChangeEvent(rect, scales[scaleIndex]));
            }
        }

        internal void MoveToPosition(Vector2 position)
        {
            var currentCenter = map.LocalToWorld(map.contentRect.center);
            var newCenter = map.UtopiaToScreen(position);
            var delta = newCenter - currentCenter;
            rect = new Rect(rect.x + delta.x, rect.y + delta.y, rect.width, rect.height);
            listener(new ViewportChangeEvent(rect, scales[scaleIndex]));
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