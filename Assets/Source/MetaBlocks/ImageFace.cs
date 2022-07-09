using System.Collections;
using Source.MetaBlocks;
using Source.MetaBlocks.ImageBlock;
using UnityEngine;
using UnityEngine.Networking;

namespace Source
{
    public class ImageFace : MetaFace
    {
        public void Init(MeshRenderer renderer, string url, ImageBlockObject block)
        {
            block.UpdateState(State.Loading);
            StartCoroutine(LoadImage(renderer.sharedMaterial, url, block, 5));
        }

        public void PlaceHolderInit(MeshRenderer renderer, MetaBlockType type, bool error)
        {
            renderer.sharedMaterial.mainTexture = type.GetIcon(error).texture;
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
                if (retries > 0 && request.responseCode == 504) // && url.StartsWith(IpfsClient.SERVER_URL))
                    yield return LoadImage(material, url, block, retries - 1);
                else block.UpdateState(State.InvalidUrlOrData);
                yield break;
            }

            var tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
            tex.Compress(false);
            material.mainTexture = tex;
            block.UpdateState(State.Ok);
        }
    }
}