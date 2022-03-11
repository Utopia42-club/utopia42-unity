using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using src.Model;
using src.Service.Ethereum;
using src.Utils;
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
            using (var webRequest = UnityWebRequest.Get(Constants.NetsURL))
            {
                yield return ExecuteRequest(webRequest, consumer, failed);
            }
        }
        
        public IEnumerator GetProfile(string walletId, Action<Profile> consumer, Action failed)
        {
            string url = Constants.ApiURL + "/profile";
            yield return (url, walletId, consumer, failed);
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, walletId))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");
                yield return ExecuteRequest(webRequest, consumer, failed);
            }
        }
        
        public IEnumerator SetLandMetadata(LandMetadata landMetadata, Action success, Action failed)
        {
            string url = Constants.ApiURL + "/land-metadata/set";
            yield return (url, landMetadata, success, failed);
        }


        internal static IEnumerator Post<B, R>(string url, B body, Action<R> success, Action failure)
        {
            using (var webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");
                yield return ExecuteRequest(webRequest, body, success, failure);
            }
        }


        internal static IEnumerator ExecuteRequest<B, T>(UnityWebRequest webRequest, B body, Action<T> success,
            Action failure)
        {
            yield return ExecuteRequest(webRequest, body, () => success(ReadResponse<T>(webRequest)), failure);
        }

        internal static IEnumerator ExecuteRequest<B>(UnityWebRequest webRequest, B body, Action success,
            Action failure)
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            var data = JsonConvert.SerializeObject(body, null, settings);
            var bodyRaw = Encoding.UTF8.GetBytes(data);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return ExecuteRequest(webRequest, success, failure);
        }

        internal static IEnumerator ExecuteRequest<T>(UnityWebRequest webRequest, Action<T> consumer, Action failed)
        {
            yield return ExecuteRequest(webRequest, () => consumer(ReadResponse<T>(webRequest)), failed);
        }


        internal static IEnumerator ExecuteRequest(UnityWebRequest webRequest, Action success, Action failed)
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
                        success();
                    else
                    {
                        failed();
                        Debug.LogError(string.Format("Request for {0} caused HTTP Error: {1}", webRequest.url,
                            webRequest.error));
                    }

                    break;
                case UnityWebRequest.Result.Success:
                    success();
                    break;
                default:
                    failed();
                    break;
            }
        }

        internal static T ReadResponse<T>(UnityWebRequest webRequest)
        {
            return webRequest.responseCode == 404
                ? default
                : JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
        }
    }
}