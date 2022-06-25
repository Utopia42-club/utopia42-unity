using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapPlayerPositionIndicator : VisualElement
    {
        private readonly Map map;
        private static readonly Sprite icon = Resources.Load<Sprite>("Icons/gps");

        public MapPlayerPositionIndicator(Map map)
        {
            this.map = map;
            AddToClassList("map-position-indicator");
            UiImageUtils.SetBackground(this, icon);
            RegisterCallback<GeometryChangedEvent>(evt => UpdatePosition());
        }

        private void UpdatePosition()
        {
            var playerPos = Player.INSTANCE.GetPosition();
            style.left = playerPos.x - contentRect.width / 2;
            style.top = -playerPos.z - contentRect.height / 2;
        }
    }
}