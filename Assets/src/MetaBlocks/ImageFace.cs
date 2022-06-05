using System.Collections;
using src.MetaBlocks;
using src.MetaBlocks.ImageBlock;
using src.Service;
using UnityEngine;
using UnityEngine.Networking;

namespace src
{
    public class ImageFace : MetaFace
    {
        public void Init(MeshRenderer renderer, string url, ImageBlockObject block)
        {
            block.UpdateState(State.Loading);
            StartCoroutine(LoadImage(renderer.sharedMaterial, url, block, 5));
        }

        private IEnumerator LoadImage(Material material, string url, ImageBlockObject block, int retries)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                block.UpdateState(State.ConnectionError);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                if (retries > 0 && request.responseCode == 504)// && url.StartsWith(IpfsClient.SERVER_URL))
                    yield return LoadImage(material, url, block, retries - 1);
                else block.UpdateState(State.InvalidUrlOrData);
                yield break;
            }

            material.mainTexture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            block.UpdateState(State.Ok);
        }
    }
}