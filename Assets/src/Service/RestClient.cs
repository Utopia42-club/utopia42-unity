using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using src.Model;
using src.Service.Ethereum;
using UnityEngine;
using UnityEngine.Networking;

namespace src.Service
{
    public class RestClient
    {
        public static RestClient INSATANCE = new RestClient();

        private RestClient()
        {
        }

        public IEnumerator LoadNetworks(Action<EthNetwork[]> consumer, Action failed)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(Utils.Constants.NetsURL))
            {
                // webRequest.SetRequestHeader("Content-Type", "application/json");
                // webRequest.SetRequestHeader("Accept", "*/*");

                yield return ExecuteRequest(consumer, failed, webRequest);
            }
        }

        public IEnumerator GetProfile(string walletId, Action<Profile> consumer, Action failed)
        {
            string url = Utils.Constants.ApiURL + "/profile";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, walletId))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");

                yield return ExecuteRequest<Profile>(consumer, failed, webRequest);
            }
        }
        
        
        public IEnumerator SetLandMetadata(LandMetadata landMetadata, Action success, Action failed)
        {
            // string url = Utils.Constants.ApiURL + "/land_metadata/set";
            const string url = "http://localhost:8080" + "/land_metadata/set";
            var data = JsonConvert.SerializeObject(landMetadata);
            var bodyRaw = Encoding.UTF8.GetBytes(data);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                
                yield return webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(string.Format("Request for {0} caused Error: {1}", webRequest.url, webRequest.error));
                        failed();
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(string.Format("Request for {0} caused HTTP Error: {1}", webRequest.url, webRequest.error));
                        failed();
                        break;
                    case UnityWebRequest.Result.Success:
                        success();
                        break;   
                }
            }
        }


        private static IEnumerator ExecuteRequest<T>(Action<T> consumer, Action failed, UnityWebRequest webRequest)
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Request for {0} caused Error: {1}", webRequest.url,
                        webRequest.error));
                    failed();
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    if (webRequest.responseCode == 404)
                        consumer(default);
                    else
                    {
                        failed();
                        Debug.LogError(string.Format("Request for {0} caused HTTP Error: {1}", webRequest.url,
                            webRequest.error));
                    }

                    break;
                case UnityWebRequest.Result.Success:
                    var result = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                    consumer(result);
                    break;
                default:
                    failed();
                    break;
            }
        }
    }
}