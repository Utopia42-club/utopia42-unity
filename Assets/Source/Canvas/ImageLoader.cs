using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Source.Canvas
{
    public class ImageLoader : MonoBehaviour
    {
        private string url = "";

        [SerializeField] private Sprite emptySprite;

        public void SetUrl(string url)
        {
            if (Equals(this.url, url))
                return;

            this.url = url;
            GetComponent<Image>().overrideSprite = emptySprite;
            if (isActiveAndEnabled && !string.IsNullOrWhiteSpace(url))
                StartCoroutine(LoadFromLikeCoroutine());
        }

        // this section will be run independently
        private IEnumerator LoadFromLikeCoroutine()
        {
            var url = this.url;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            yield return request.SendWebRequest();
            if (!Equals(this.url, url)) yield break;

            if (request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.ConnectionError)
                GetComponent<Image>().overrideSprite = emptySprite;
            else
            {
                // ImageComponent.texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
                Texture2D tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
                if (tex != null)
                {
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(tex.width / 2, tex.height / 2));
                    GetComponent<Image>().overrideSprite = sprite;
                }
            }
        }
    }
}