using System;
using Source.Canvas;
using Source.Model;
using Source.Ui.Menu;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Position = UnityEngine.UIElements.Position;

namespace Source.Ui.Map
{
    internal class MapLand : VisualElement
    {
        private readonly Land land;
        private static readonly Sprite nftLogo = Resources.Load<Sprite>("Icons/nft-logo");
        private readonly Map map;

        public MapLand(Land land, Map map)
        {
            this.land = land;
            this.map = map;

            var landColor = Colors.GetLandColor(land);
            if (landColor != null)
                style.backgroundColor = new StyleColor(landColor.Value);
            AddToClassList(Colors.GetLandBorderStyle(land));
            AddToClassList("map-land");
            UpdateRect();

            if (land is {isNft: true})
            {
                const int nftLogoDefaultSize = 30;
                var width = style.width.value.value;
                var height = style.height.value.value;
                var visualElement = new VisualElement
                {
                    style =
                    {
                        width = Math.Min(width - 6, nftLogoDefaultSize), // -6 is for border and position
                        height = Math.Min(height - 6, nftLogoDefaultSize), // -6 is for border and position
                        position = new StyleEnum<Position>(Position.Absolute),
                        bottom = 2,
                        right = 2
                    }
                };
                UiImageLoader.SetBackground(visualElement, nftLogo);
                Add(visualElement);
            }
        }

        internal void UpdateRect()
        {
            var end = land.endCoordinate.ToVector3();
            var start = land.startCoordinate.ToVector3();
            var diag = end - start;
            style.top = -end.z;
            style.left = start.x;
            style.width = diag.x;
            style.height = diag.z;
        }

        internal Land GetLand()
        {
            return land;
        }


        // private static string GetLandOutlineColor(Land land)
        // {
        // var owner = land.owner.Equals(Settings.WalletId());
        // return owner
        //     ? (land.isNft ? MAP_OWNED_LAND_NFT : MAP_OWNED_LAND)
        //     : (land.isNft ? MAP_OTHERS_LAND_NFT : MAP_OTHERS_LAND);
        // }


        internal static Vector2Int RoundDown(Vector2 v)
        {
            return new Vector2Int(RoundDown(v.x), RoundDown(v.y));
        }

        internal static Vector3Int RoundDown(Vector3 v)
        {
            return new Vector3Int(RoundDown(v.x), RoundDown(v.y), RoundDown(v.z));
        }

        internal static Vector3Int RoundUp(Vector3 v)
        {
            return new Vector3Int(RoundUp(v.x), RoundUp(v.y), RoundUp(v.z));
        }

        internal static int RoundDown(float x)
        {
            return 5 * (int) Mathf.Floor(x / 5);
        }

        internal static int RoundUp(float x)
        {
            return 5 * (int) Mathf.Ceil(x / 5);
        }
    }
}