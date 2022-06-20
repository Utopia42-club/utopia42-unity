using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapPointerPositionLabel : Label
    {
        private readonly Map map;

        public MapPointerPositionLabel(Map map)
        {
            this.map = map;
            AddToClassList("map-mouse-label");
            map.RegisterCallback<MouseMoveEvent>(e => OnPositionChanged(e.mousePosition));
            map.RegisterCallback<MouseLeaveEvent>(e => visible = false);
            map.RegisterCallback<MouseEnterEvent>(e => visible = true);
        }

        private void OnPositionChanged(Vector2 mousePosition)
        {
            var utPos = map.ScreenToUtopia(mousePosition);
            text = $"{Mathf.FloorToInt(utPos.x)}, {Mathf.FloorToInt(utPos.y)}";
            mousePosition = map.WorldToLocal(mousePosition);
            style.left = Mathf.Min(map.contentRect.width - contentRect.width - 8,  mousePosition.x);
            style.top = Mathf.Max(0, mousePosition.y - contentRect.height - 8);
        }
    }
}