using Source.Ui.Popup;
using Source.Ui.Utils;
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
            var zoomOutButton = this.Q<Button>("zoomOutButton");
            zoomOutButton.AddManipulator(new ToolTipManipulator(Side.TopLeft));
            var currentLocationLabel = this.Q<Label>("currentLocationLabel");
            currentLocationLabel.text = Player.INSTANCE.GetPosition().ToString();
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
                    evt.StopImmediatePropagation();
            });
        }
    }
}