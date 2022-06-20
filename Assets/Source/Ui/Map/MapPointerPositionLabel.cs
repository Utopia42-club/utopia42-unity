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
        }

        private void OnPositionChanged(Vector2 mousePosition)
        {
            var utPos = map.ScreenToUtopia(mousePosition);
            text = $"{utPos.x}, ${utPos.y}";
            // style.top = mousePosition.
        }
    }
}