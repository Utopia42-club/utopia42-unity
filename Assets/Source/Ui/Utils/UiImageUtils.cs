using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public class UiImageUtils
    {
        public static IEnumerator LoadImage(string url, Action<Sprite> onSuccess, Action onFail)
        {
            if (string.IsNullOrWhiteSpace(url)) yield break;

            var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.ConnectionError)
                onFail();
            else
            {
                var tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
                if (tex != null)
                {
                    var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(tex.width / 2, tex.height / 2));
                    onSuccess(sprite);
                }
            }
        }

        public static IEnumerator SetBackGroundImageFromUrl(string url, Sprite emptySprite, VisualElement visualElement,
            Action onDone = null, Action onFail = null)
        {
            SetBackground(visualElement, emptySprite);
            yield return LoadImage(url, sprite =>
                {
                    SetBackground(visualElement, sprite);
                    onDone?.Invoke();
                },
                () =>
                {
                    SetBackground(visualElement, Resources.Load<Sprite>("Icons/error"));
                    onFail?.Invoke();
                });
        }

        public static void SetBackground(VisualElement visualElement, Sprite sprite)
        {
            var background = new StyleBackground();
            background.value = Background.FromSprite(sprite);
            visualElement.style.backgroundImage = background;
        }
    }
}