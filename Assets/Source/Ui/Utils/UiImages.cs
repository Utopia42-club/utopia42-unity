using System;
using System.Collections;
using Source.Ui.Loading;
using Source.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Source.Ui.Utils
{
    public static class UiImageUtils
    {
        private static IEnumerator LoadImage(string url, Action<Texture2D> onSuccess, Action onFail)
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
                    Textures.TryCompress(tex);
                    onSuccess(tex);
                }
            }
        }

        public static IEnumerator SetBackGroundImageFromUrl(string url, VisualElement visualElement,
            Action onDone = null, Action onFail = null, bool showLoading = true)
        {
            yield return SetBackGroundImageFromUrl(url, null, false, visualElement, onDone, onFail, showLoading);
        }

        public static IEnumerator SetBackGroundImageFromUrl(string url, Sprite emptySprite, bool destroyEmptySprite,
            VisualElement visualElement,
            Action onDone = null, Action onFail = null, bool showLoading = true)
        {
            bool detached = false;
            EventCallback<DetachFromPanelEvent> listener = e => detached = true;
            visualElement.RegisterCallback(listener);

            if (emptySprite != null)
                SetBackground(visualElement, emptySprite.texture, destroyEmptySprite);
            LoadingController loading = null;
            if (showLoading)
                loading = LoadingLayer.Show(visualElement);
            yield return LoadImage(url, tex =>
                {
                    visualElement.UnregisterCallback(listener);

                    loading?.Close();
                    if (emptySprite != null && destroyEmptySprite)
                        Object.Destroy(emptySprite);
                    if (detached)
                    {
                        Object.Destroy(tex);
                    }
                    else
                    {
                        SetBackground(visualElement, tex, true);
                        onDone?.Invoke();
                    }
                },
                () =>
                {
                    visualElement.UnregisterCallback(listener);
                    loading?.Close();
                    if (emptySprite != null && destroyEmptySprite)
                        Object.Destroy(emptySprite);
                    if (!detached)
                    {
                        SetBackground(visualElement, Resources.Load<Sprite>("Icons/error"), false);
                        onFail?.Invoke();
                    }
                });
        }

        public static void SetBackground(VisualElement visualElement, Sprite sprite, bool destroyOnDetach,
            ScaleMode? scaleMode = null)
        {
            SetBackground(visualElement, sprite.texture, destroyOnDetach, scaleMode);
            if (destroyOnDetach)
                visualElement.RegisterCallback<DetachFromPanelEvent>(e => Object.Destroy(sprite));
        }

        public static void SetBackground(VisualElement visualElement, Texture2D tex, bool destroyOnDetach,
            ScaleMode? scaleMode = null)
        {
            var background = new StyleBackground();
            background.value = Background.FromTexture2D(tex);
            visualElement.style.backgroundImage = background;
            if (scaleMode != null)
                visualElement.style.unityBackgroundScaleMode = scaleMode.Value;
            if (destroyOnDetach)
                visualElement.RegisterCallback<DetachFromPanelEvent>(e => Object.Destroy(tex));
        }
    }
}