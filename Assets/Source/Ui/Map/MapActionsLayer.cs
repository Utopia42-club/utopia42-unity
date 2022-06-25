using Source.Ui.Popup;
using Source.Ui.Utils;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapActionsLayer : UxmlElement
    {
        public MapActionsLayer(Map map) : base("Ui/Map/MapActionsLayer")
        {
            var zoomButtons = this.Q<VisualElement>("zoomButtons");
            var zoomInButton = this.Q<Button>("zoomInButton");
            zoomInButton.clickable.clicked += map.ZoomIn;
            var zoomOutButton = this.Q<Button>("zoomOutButton");
            zoomOutButton.clickable.clicked += map.ZoomOut;
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
                    || zoomButtons.worldBound.Contains(evt.mousePosition))
                    evt.StopImmediatePropagation();
            });
        }
    }
}