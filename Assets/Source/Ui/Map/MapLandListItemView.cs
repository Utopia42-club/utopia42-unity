using Source.Canvas;
using Source.Model;
using Source.Service;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapLandListItemView : UxmlElement
    {
        private readonly Land land;

        public MapLandListItemView(Land land, Map map) : base("Ui/Map/LandListItemView")
        {
            this.land = land;
            var nameLabel = this.Q<Label>("landNameLabel");
            var sizeLabel = this.Q<Label>("landSizeLabel");
            var coordinateLabel = this.Q<Label>("landCoordinatesLabel");
            var colorBar = this.Q<VisualElement>("colorBar");
            var nftLogo = this.Q<VisualElement>("nftLogo");

            var name = land.GetName();
            var s = name != null && name.Trim().Length > 0 ? name : "Land";
            if (s.Length > 13)
                s = s[..13] + "...";
            nameLabel.text = s + " " + "#" + land.id;

            if (WorldService.INSTANCE.IsLandChanged(land))
                nameLabel.text += "*";
            
            var start = land.startCoordinate;
            var end = land.endCoordinate;
            coordinateLabel.text = $"({start.x}, {start.z}, {end.x}, {end.z})";

            sizeLabel.text = GetLandSize().ToString();
            nftLogo.style.display = land.isNft ? DisplayStyle.Flex : DisplayStyle.None;
            colorBar.style.backgroundColor = Colors.GetLandColor(land) ?? Colors.MAP_DEFAULT_LAND_COLOR;
            
            RegisterCallback<MouseDownEvent>(evt =>
            {
                evt.StopPropagation();
                map.MoveTo(land);
            });
        }

        private long GetLandSize()
        {
            var rect = land.ToRect();
            return (long) (rect.width * rect.height);
        }
    }
}