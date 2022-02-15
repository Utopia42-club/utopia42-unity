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
        public Button button;

        private void Start()
        {
            button.onClick.AddListener(() => GameManager.INSTANCE.NavigateInMap(land));
        }

        public void SetLand(Land land)
        {
            this.land = land;
            landIdLabel.SetText("#" + land.id);
            coordinateLabel.SetText("(" + land.x1 + ", " + land.y1 + ", " + land.x2 + ", " + land.y2 + ")");
            sizeLabel.SetText(GetLandSize(land).ToString());
            nftToggle.SetActive(land.isNft);
        }

        private long GetLandSize(Land land1)
        {
            return (land1.x2 - land1.x1) * (land1.y2 - land1.y1);
        }
    }
}