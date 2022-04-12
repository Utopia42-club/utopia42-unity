using System.Collections;
using src.MetaBlocks;
using src.MetaBlocks.ImageBlock;
using src.Service;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace src
{
    public class ImageFace : MetaFace
    {
        public void Init(MeshRenderer renderer, string url, ImageBlockObject block, Voxels.Face face)
        {
            block.UpdateStateAndIcon(StateMsg.Loading, face);
            StartCoroutine(LoadImage(renderer.sharedMaterial, url, block, face, 5));
        }

        private IEnumerator LoadImage(Material material, string url, ImageBlockObject block, Voxels.Face face,
            int retries)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("loading image from: " + request.url + ", caused error: " + request.error);
                block.UpdateStateAndIcon(StateMsg.ConnectionError, face);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("loading image from: " + request.url + ", caused error: " + request.error);
                if (retries > 0 && request.responseCode == 504)// && url.StartsWith(IpfsClient.SERVER_URL))
                    yield return LoadImage(material, url, block, face, retries - 1);
                else block.UpdateStateAndIcon(StateMsg.InvalidUrlOrData, face);
                yield break;
            }

            material.mainTexture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            block.UpdateStateAndIcon(StateMsg.Ok, face);
        }
    }
}