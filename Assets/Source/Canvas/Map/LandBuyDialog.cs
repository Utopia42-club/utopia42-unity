using System.Collections.Generic;
using Source.Model;
using Source.Service.Ethereum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Source.Canvas.Map
{
    public class LandBuyDialog : MonoBehaviour
    {
        public TextMeshProUGUI landSizeLabel;
        public TextMeshProUGUI landPriceLabel;
        public Button buyButton;
        public Button cancelButton;
        public Map map;

        private GameManager manager;
        private RectTransform rectTransform;
        private Land land;

        void Start()
        {
            manager = GameManager.INSTANCE;
            cancelButton.onClick.AddListener(Close);
            buyButton.onClick.AddListener(DoBuy);
        }

        private void DoBuy()
        {
            var lands = new List<Land> {land};
            GameManager.INSTANCE.Buy(lands);
        }

        private void Close()
        {
            map.CloseLandBuyDialogState();
        }

        public void SetRect(RectTransform rectTransform)
        {
            var land = new Land();
            var rect = rectTransform.rect;
            var localPosition = rectTransform.localPosition;
            land.startCoordinate = new SerializableVector3Int((int) localPosition.x, 0, (int) localPosition.y);
            land.endCoordinate = new SerializableVector3Int((int) localPosition.x + (int) rect.width, 0,
                (int) localPosition.y + (int) rect.height);

            this.land = land;
            landSizeLabel.SetText((rect.width * rect.height).ToString());
            landPriceLabel.SetText("Calculating...");
            StartCoroutine(EthereumClientService.INSTANCE.GetLandPrice(land.startCoordinate.x, land.endCoordinate.x,
                land.startCoordinate.z, land.endCoordinate.z,
                price => { landPriceLabel.SetText(price.ToString()); }, () =>
                {
                    GameManager.INSTANCE.ShowConnectionError();
                }));
        }
    }
}