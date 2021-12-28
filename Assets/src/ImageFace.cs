using System.Collections;
using src.MetaBlocks.ImageBlock;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace src
{
    public class ImageFace : MetaFace
    {
        public void Init(MeshRenderer renderer, string url)
        {
            StartCoroutine(LoadImage(renderer.material, url));
        }

        private IEnumerator LoadImage(Material material, string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                yield break;
            material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}
