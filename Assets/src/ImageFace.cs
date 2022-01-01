using System.Collections;
using src.MetaBlocks;
using src.MetaBlocks.ImageBlock;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace src
{
    public class ImageFace : MetaFace
    {
        public void Init(MeshRenderer renderer, string url, ImageBlockObject block, int faceIndex)
        {
            block.UpdateStateAndIcon(faceIndex, StateMsg.Loading);
            StartCoroutine(LoadImage(renderer.material, url, block, faceIndex));
        }

        private IEnumerator LoadImage(Material material, string url, ImageBlockObject block, int faceIndex)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ProtocolError || 
                request.result == UnityWebRequest.Result.ConnectionError)
            {
                block.UpdateStateAndIcon(faceIndex, StateMsg.InvalidUrl); // TODO ?
                yield break;
            }
            if (request.result == UnityWebRequest.Result.DataProcessingError)
            {
                block.UpdateStateAndIcon(faceIndex, StateMsg.InvalidData);
                yield break;
            }
            material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            block.UpdateStateAndIcon(faceIndex, StateMsg.Ok);
        }
    }
}
