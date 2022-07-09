using UnityEngine;

namespace Source.Utils
{
    public static class Textures
    {
        public static void TryCompress(Texture2D texture)
        {
            if (texture.width % 4 == 0 && texture.height % 4 == 0)
                texture.Compress(false);
        }
    }
}