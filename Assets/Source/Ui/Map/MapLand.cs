using Source.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    internal class MapLand : VisualElement
    {
        private readonly Land land;

        public MapLand(Land land)
        {
            this.land = land;
            AddToClassList("map-land");
            UpdateRect();
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