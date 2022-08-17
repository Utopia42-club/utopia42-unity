using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapPlayerPositionIndicator : VisualElement
    {
        private readonly Map map;
        private readonly EventCallback<GeometryChangedEvent> geomListener;
        private static readonly Sprite icon = Resources.Load<Sprite>("Icons/gps");

        public MapPlayerPositionIndicator(Map map)
        {
            this.map = map;
            AddToClassList("map-position-indicator");
            UiImageUtils.SetBackground(this, icon, false);
            pickingMode = PickingMode.Ignore;
            geomListener = evt => GeometryChanged();
            RegisterCallback(geomListener);
            RegisterCallback<AttachToPanelEvent>(e =>
            {
                var subs = map.scaleObservable.Subscribe(scale => UpdatePosition(30 / scale, 30 / scale));
                RegisterCallback<DetachFromPanelEvent>(e => subs.Unsubscribe());
            });
        }

        private void GeometryChanged()
        {
            UnregisterCallback(geomListener);
            UpdatePosition(30, 30);
        }

        private void UpdatePosition(float width, float height)
        {
            var playerPos = Player.INSTANCE.GetPosition();
            style.width = width;
            style.height = height;
            style.left = playerPos.x - width / 2;
            style.top = -playerPos.z - height / 2;
        }
    }
}