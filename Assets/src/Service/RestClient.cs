using System;
using System.Collections;
using Newtonsoft.Json;
using src.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace src.Service
{
    public class RestClient
    {
        public static readonly string SERVER_URL = "http://app.utopia42.club:5025/";

        public static RestClient INSATANCE = new RestClient();
        private RestClient()
        {
        }

        public IEnumerator GetProfile(string walletId, Action<Profile> consumer, Action failed)
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
                        failed();
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        if (webRequest.responseCode == 404)
                            consumer(null);
                        else
                        {
                            failed();
                            Debug.LogError(string.Format("Post for {0} caused HTTP Error: {1}", url, webRequest.error));
                        }
                        break;
                    case UnityWebRequest.Result.Success:
                        var details = JsonConvert.DeserializeObject<Profile>(webRequest.downloadHandler.text);
                        consumer(details);
                        break;
                    default:
                        failed();
                        break;
                }
            }
            yield break;
        }
    }
}

