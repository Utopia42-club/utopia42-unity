using System.Collections.Generic;
using src.Model;
using src.Service.Ethereum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class LandBuyDialog : MonoBehaviour
    {
        public ActionButton closeButton;
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
            closeButton.AddListener(Close);
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
            land.x1 = (long) localPosition.x;
            land.y1 = (long) localPosition.y;
            land.x2 = land.x1 + (long) rect.width;
            land.y2 = land.y1 + (long) rect.height;
            
            this.land = land;
            landSizeLabel.SetText((rect.width * rect.height).ToString());
            landPriceLabel.SetText("Computing...");
            StartCoroutine(EthereumClientService.INSTANCE.GetLandPrice(land.x1, land.x2, land.y1, land.y2, price =>
            {
                landPriceLabel.SetText(price.ToString());
            }));
        }
    }
}