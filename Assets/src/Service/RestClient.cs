using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RestClient
{
    public static readonly string SERVER_URL = "http://horizon.madreza.ir:8082/";

    public static RestClient INSATANCE = new RestClient();
    private RestClient()
    {
    }

    public IEnumerator GetProfile(string walletId, Action<Profile> consumer)
    {
        string url = SERVER_URL + "profile";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, walletId))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "*/*");

            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Post for {0} caused Error: {1}", url, webRequest.error));
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    if (webRequest.responseCode == 404)
                    {
                        consumer.Invoke(null);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Post for {0} caused HTTP Error: {1}", url, webRequest.error));
                    }
                    break;
                case UnityWebRequest.Result.Success:
                    var details = JsonConvert.DeserializeObject<Profile>(webRequest.downloadHandler.text);
                    consumer.Invoke(details);
                    break;
            }
        }
        yield break;
    }
}

