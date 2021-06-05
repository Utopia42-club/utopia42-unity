using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class IpfsClient
{
    private static readonly string GET_URL = "https://utopia42.club/api/v0/cat?arg=/ipfs/";

    public static IpfsClient INSATANCE = new IpfsClient();
    private IpfsClient()
    {
    }

    public IEnumerator GetLandDetails(string id, Action<LandDetails> consumer)
    {
        string url = GET_URL + id;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Get for {0} caused Error: {1}", url, webRequest.error));
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(string.Format("Get for {0} caused HTTP Error: {1}", url, webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:
                    var details = JsonConvert.DeserializeObject<LandDetails>(webRequest.downloadHandler.text);
                    consumer.Invoke(details);
                    break;
            }
        }
        yield break;
    }

}
