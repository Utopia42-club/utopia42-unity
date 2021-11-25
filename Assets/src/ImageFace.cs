using System.Collections;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace src
{
    public class ImageFace : MetaFace
    {

        private void Start()
        {
            //Init(Voxels.Face.FRONT, "https://upload.wikimedia.org/wikipedia/commons/7/78/Image.jpg", 2, 2);
        }

        public void Init(Voxels.Face face, string url, int width, int height)
        {
            MeshRenderer meshRenderer = Initialize(face, width, height);
            StartCoroutine(LoadImage(meshRenderer.material, url));
        }

        private IEnumerator LoadImage(Material material, string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                yield break;
            material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            yield break;
        }
    }
}
