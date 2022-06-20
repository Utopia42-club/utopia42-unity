using log4net.Appender;
using Source.Canvas;
using Source.Model;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    internal class MapLand : VisualElement
    {
        public MapLand(Land land)
        {
            AddToClassList("map-land");
            style.top = 0;
            style.left = 0;
            style.width = 100;
            style.height = 100;
        }
        
        
        // private static string GetLandOutlineColor(Land land)
        // {
            // var owner = land.owner.Equals(Settings.WalletId());
            // return owner
            //     ? (land.isNft ? MAP_OWNED_LAND_NFT : MAP_OWNED_LAND)
            //     : (land.isNft ? MAP_OTHERS_LAND_NFT : MAP_OTHERS_LAND);
        // }
    }
}