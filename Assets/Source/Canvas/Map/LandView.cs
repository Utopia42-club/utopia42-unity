using Source.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Source.Canvas.Map
{
    public class LandView : MonoBehaviour
    {
        private Land land;

        public TextMeshProUGUI coordinateLabel;
        public TextMeshProUGUI sizeLabel;
        public TextMeshProUGUI nameLabel;
        public GameObject nftToggle;
        public Image colorImage;
        public Button button;

        private void Start()
        {
            button.onClick.AddListener(() => GameManager.INSTANCE.NavigateInMap(land));
        }

        public void SetLand(Land land)
        {
            this.land = land;
            var name = land.GetName();
            var s = (name != null && name.Trim().Length > 0 ? name : "Land");
            if (s.Length > 13)
                s = s.Substring(0, 13) + "...";
            nameLabel.SetText(s + " " + "#" + land.id);

            var start = land.startCoordinate;
            var end = land.startCoordinate;
            coordinateLabel.SetText($"({start.x}, {start.z}, {end.x}, {end.z})");
            sizeLabel.SetText(GetLandSize().ToString());
            nftToggle.SetActive(land.isNft);
            colorImage.color = Colors.GetLandColor(land);
        }

        private long GetLandSize()
        {
            var rect = land.ToRect();
            return (long) (rect.width * rect.height);
        }
    }
}