﻿using Source.Model;
using Source.Service.Auth;
using UnityEngine;

namespace Source.Canvas
{
    public static class Colors
    {
        public static readonly Color PRIMARY_COLOR = new(44 / 255f, 62 / 255f, 80 / 255f);
        public static readonly Color SECONDARY_COLOR = new(52 / 255f, 73 / 255f, 94 / 255f);
        public static readonly Color MAP_BACKGROUND = new(236 / 255f, 240 / 255f, 241 / 255f);
        public static readonly Color MAP_OWNED_LAND = new(22 / 255f, 160 / 255f, 133 / 255f);
        public static readonly Color MAP_OWNED_LAND_NFT = new(241 / 255f, 196 / 255f, 15 / 255f);
        public static readonly Color MAP_OTHERS_LAND = new(149 / 255f, 165 / 255f, 166 / 255f);
        public static readonly Color MAP_OTHERS_LAND_NFT = MAP_OTHERS_LAND;
        public static readonly Color MAP_GRID_LINES = new(41 / 255f, 128 / 255f, 185 / 255f, 0.2f);
        public static readonly Color MAP_GRID_ORIGIN_LINES = new(192 / 255f, 57 / 255f, 43 / 255f);
        public static readonly Color MAP_DEFAULT_LAND_COLOR = MAP_OTHERS_LAND;
        public static readonly Color TRANSPARENT = new(0, 0, 0, 255);

        public static Color? ConvertHexToColor(string hex)
        {
            var validColor = ColorUtility.TryParseHtmlString(hex, out var color);
            if (validColor)
                return color;
            return null;
        }

        public static string ConvertToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        public static Color? GetLandColor(Land land)
        {
            Color? color = null;
            if (land != null && land.properties != null && land.properties.color != null)
                color = ConvertHexToColor(land.properties.color);

            return color;
        }

        public static Color GetLandOutlineColor(Land land)
        {
            var owner = AuthService.Instance.IsCurrentUser(land.owner);
            return owner
                ? land.isNft ? MAP_OWNED_LAND_NFT : MAP_OWNED_LAND
                : land.isNft
                    ? MAP_OTHERS_LAND_NFT
                    : MAP_OTHERS_LAND;
        }

        public static string GetLandBorderStyle(Land land)
        {
            var owner = land.owner.Equals(AuthService.Instance.WalletId());
            return owner
                ? land.isNft ? "map-owned-land-nft" : "map-owned-land"
                : land.isNft
                    ? "map-others-land-nft"
                    : "map-others-land";
        }
    }
}