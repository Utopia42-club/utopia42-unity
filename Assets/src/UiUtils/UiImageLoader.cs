using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace src.Canvas
{
    public class UiImageLoader
    {
        public static IEnumerator SetBackGroundImageFromUrl(string url, Sprite emptySprite, VisualElement visualElement)
        {
            SetBackground(visualElement, emptySprite);

            if (string.IsNullOrWhiteSpace(url)) yield break;

            var request = UnityWebRequestTexture.GetTexture(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.ConnectionError)
                SetBackground(visualElement, Resources.Load<Sprite>("Icons/error"));
            else
            {
                var tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
                if (tex != null)
                {
                    var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(tex.width / 2, tex.height / 2));
                    SetBackground(visualElement, sprite);
                }
            }
        }

        private static void SetBackground(VisualElement visualElement, Sprite sprite)
        {
            var background = new StyleBackground();
            background.value = Background.FromSprite(sprite);
            visualElement.style.backgroundImage = background;
        }
    }
}