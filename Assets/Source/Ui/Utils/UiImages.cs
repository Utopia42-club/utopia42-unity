using System;
using System.Collections;
using Source.Ui.LoadingLayer;
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

        public static IEnumerator SetBackGroundImageFromUrl(string url, VisualElement visualElement,
            Action onDone = null, Action onFail = null, bool showLoading = true)
        {
            yield return SetBackGroundImageFromUrl(url, null, visualElement, onDone, onFail, showLoading);
        }

        public static IEnumerator SetBackGroundImageFromUrl(string url, Sprite emptySprite, VisualElement visualElement,
            Action onDone = null, Action onFail = null, bool showLoading = true)
        {
            if (emptySprite != null)
                SetBackground(visualElement, emptySprite);
            LoadingController loading = null;
            if (showLoading)
                loading = LoadingLayer.LoadingLayer.Show(visualElement);
            yield return LoadImage(url, sprite =>
                {
                    loading?.Close();
                    SetBackground(visualElement, sprite);
                    onDone?.Invoke();
                },
                () =>
                {
                    loading?.Close();
                    SetBackground(visualElement, Resources.Load<Sprite>("Icons/error"));
                    onFail?.Invoke();
                });
        }

        public static void SetBackground(VisualElement visualElement, Sprite sprite, ScaleMode? scaleMode = null)
        {
            var background = new StyleBackground();
            background.value = Background.FromSprite(sprite);
            visualElement.style.backgroundImage = background;
            if (scaleMode != null)
                visualElement.style.unityBackgroundScaleMode = scaleMode.Value;
        }
    }
}