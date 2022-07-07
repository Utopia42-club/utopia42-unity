using Source.Model;
using Source.Service.Ethereum;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class LandBuyDialog : UxmlElement
    {
        public LandBuyDialog(Land land) : base(typeof(LandBuyDialog))
        {
            var sizeLabel = this.Q<Label>("sizeLabel");
            var priceLabel = this.Q<Label>("priceLabel");
            sizeLabel.text = (land.ToRect().width * land.ToRect().height).ToString();
            priceLabel.text = "Calculating...";
            GameManager.INSTANCE.StartCoroutine(
                EthereumClientService.INSTANCE.GetLandPrice(
                    land.startCoordinate.x, land.endCoordinate.x, land.startCoordinate.z, land.endCoordinate.z,
                    price => priceLabel.text = price.ToString(),
                    () => GameManager.INSTANCE.ShowConnectionError())
            );
        }
    }
}