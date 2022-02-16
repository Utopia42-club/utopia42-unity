using UnityEngine;

namespace src.Canvas
{
    public static class Colors
    {
        public static readonly Color PRIMARY_COLOR = new Color(44 / 255f, 62 / 255f, 80 / 255f);
        public static readonly Color SECONDARY_COLOR = new Color(52 / 255f, 73 / 255f, 94 / 255f);
        public static readonly Color MAP_BACKGROUND = new Color(236 / 255f, 240 / 255f, 241 / 255f);
        public static readonly Color MAP_OWNED_LAND = new Color(22 / 255f, 160 / 255f, 133 / 255f);
        public static readonly Color MAP_OWNED_LAND_NFT = new Color(241 / 255f, 196 / 255f, 15 / 255f);
        public static readonly Color MAP_OTHERS_LAND = new Color(149 / 255f, 165 / 255f, 166 / 255f);
        public static readonly Color MAP_OTHERS_LAND_NFT = MAP_OTHERS_LAND;
        public static readonly Color MAP_GRID_LINES = new Color(41 / 255f, 128 / 255f, 185 / 255f, 0.6f);
        public static readonly Color MAP_GRID_ORIGIN_LINES = new Color(192 / 255f, 57 / 255f, 43 / 255f);
        public static readonly Color MAP_DEFAULT_LAND_COLOR = new Color(149 / 255f, 165 / 255f, 166 / 255f);
        
        public static  Color? ConvertHexToColor(string hex)
        {
            var validColor = ColorUtility.TryParseHtmlString(hex, out var color);
            if (validColor)
                return color;
            return null;
        }
    }
}