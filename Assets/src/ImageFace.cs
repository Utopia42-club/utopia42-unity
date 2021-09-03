using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ImageFace : MetaFace
{
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
