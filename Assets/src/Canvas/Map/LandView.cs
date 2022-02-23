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
        public TextMeshProUGUI nameLabel;
        public GameObject nftToggle;
        public GameObject byIdSection;
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
            if (name != null && name.Trim().Length > 0)
            {
                nameLabel.SetText(name);
                byIdSection.SetActive(false);
            }
            else
            {
                landIdLabel.SetText("#" + land.id);
                byIdSection.SetActive(true);
                nameLabel.gameObject.SetActive(false);
            }

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