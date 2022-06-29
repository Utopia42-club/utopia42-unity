using Source.Ui.Popup;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapActionsLayer : UxmlElement
    {
        public MapActionsLayer(Map map) : base("Ui/Map/MapActionsLayer")
        {
            var actions = this.Q<VisualElement>("actions");
            var showYourLocationButton = this.Q<Button>("showYourLocationButton");
            showYourLocationButton.clickable.clicked += map.MoveToPlayerPosition;
            showYourLocationButton.tooltip = "Show your location";
            showYourLocationButton.AddManipulator(new ToolTipManipulator(Side.TopLeft));

            var zoomInButton = this.Q<Button>("zoomInButton");
            zoomInButton.AddManipulator(new ToolTipManipulator(Side.TopLeft));
            zoomInButton.clickable.clicked += map.ZoomIn;

            var zoomOutButton = this.Q<Button>("zoomOutButton");
            zoomOutButton.AddManipulator(new ToolTipManipulator(Side.TopLeft));
            zoomOutButton.clickable.clicked += map.ZoomOut;

            var currentLocationLabel = this.Q<Label>("currentLocationLabel");
            var pos = Player.INSTANCE.GetPosition();
            currentLocationLabel.text = new Vector3Int((int) pos.x, (int) pos.y, (int) pos.z).ToString();
            var currentLocationBox = this.Q<VisualElement>("currentLocationBox");
            currentLocationBox.tooltip = "Click to copy";
            currentLocationBox.AddManipulator(new ToolTipManipulator(Side.TopLeft));
            currentLocationBox.RegisterCallback<MouseMoveEvent>(evt => GameManager.INSTANCE.CopyPositionLink());

            style.position = Position.Absolute;
            style.overflow = Overflow.Visible;
            style.width = style.height = 0;
            style.right = style.bottom = 0;

            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (currentLocationBox.worldBound.Contains(evt.mousePosition)
                    || actions.worldBound.Contains(evt.mousePosition))
                    evt.StopPropagation();
            });
        }
    }
}