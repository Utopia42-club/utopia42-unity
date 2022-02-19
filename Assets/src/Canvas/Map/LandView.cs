using src;
using src.Canvas;
using src.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class LandView : MonoBehaviour
    {
        private Land land;

        public TextMeshProUGUI landIdLabel;
        public TextMeshProUGUI coordinateLabel;
        public TextMeshProUGUI sizeLabel;
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
            landIdLabel.SetText("#" + land.id);

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