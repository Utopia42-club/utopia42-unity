using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IpfsClient
{
    private static readonly string SERVER_URL = "https://utopia42.club/api/v0";

    public static IpfsClient INSATANCE = new IpfsClient();
    private IpfsClient()
    {
    }

    public IEnumerator GetLandDetails(string id, Action<LandDetails> consumer)
    {
        yield break;
        string url = SERVER_URL + "/cat?arg=/ipfs/" + id;
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

    public IEnumerator Upload(List<LandDetails> details, Action<List<string>> success)
    {
        var result = new List<string>();
        string url = SERVER_URL + "/add?stream-channels=true&progress=false";
        foreach (var detail in details)
        {
            if (string.IsNullOrWhiteSpace(detail.region.ipfsKey) && detail.changes.Count == 0)
            {
                result.Add(detail.region.ipfsKey);
                continue;
            }

            string data = JsonConvert.SerializeObject(detail);
            Debug.Log(data);
            var form = new List<IMultipartFormSection>();
            form.Add(new MultipartFormDataSection("file", data));
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)//FIXME add proper error handling
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(string.Format("Posing data for {0} caused Error: {1}", url, webRequest.error));
                        result.Add(detail.region.ipfsKey);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(string.Format("Get for {0} caused HTTP Error: {1}", url, webRequest.error));
                        result.Add(detail.region.ipfsKey);
                        break;
                    case UnityWebRequest.Result.Success:
                        var response = JsonConvert.DeserializeObject<IpfsResponse>(webRequest.downloadHandler.text);
                        result.Add(response.hash);
                        break;
                }
            }
        }
        success.Invoke(result);
        yield break;
    }

}


[System.Serializable]
class IpfsResponse
{
    public string name;
    public string hash;
    public string size;
}
