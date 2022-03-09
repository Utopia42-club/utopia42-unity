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
        public void Init(MeshRenderer renderer, string url, ImageBlockObject block, Voxels.Face face)
        {
            block.UpdateStateAndIcon(StateMsg.Loading, face);
            StartCoroutine(LoadImage(renderer.sharedMaterial, url, block, face));
        }

        private IEnumerator LoadImage(Material material, string url, ImageBlockObject block, Voxels.Face face)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                block.UpdateStateAndIcon(StateMsg.ConnectionError, face);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                block.UpdateStateAndIcon(StateMsg.InvalidUrlOrData, face);
                yield break;
            }

            material.mainTexture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            block.UpdateStateAndIcon(StateMsg.Ok, face);
        }
    }
}