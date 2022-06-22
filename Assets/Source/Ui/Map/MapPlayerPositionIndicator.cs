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
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var playerPos = Player.INSTANCE.GetPosition();
            var screenPos = map.UtopiaToScreen(new Vector2(playerPos.x, playerPos.z));
            screenPos = map.WorldToLocal(screenPos);
            style.left = Mathf.Min(map.contentRect.width - contentRect.width - 8, screenPos.x);
            style.top = Mathf.Max(0, screenPos.y - contentRect.height - 8);
        }
    }
}