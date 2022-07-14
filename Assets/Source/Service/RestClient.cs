using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Source.Service
{
    public class RestClient
    {
        private const string TOKEN_HEADER_KEY = "X-Auth-Token";

        internal static IEnumerator Post<TB>(string url, TB body, Action success, Action failure,
            string authToken = null)
        {
            using (var webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");
                if (authToken != null)
                    webRequest.SetRequestHeader(TOKEN_HEADER_KEY, authToken);
                yield return ExecuteRequest(webRequest, body, success, failure);
            }
        }

        internal static IEnumerator Post<TB, TR>(string url, TB body, Action<TR> success, Action failure,
            string authToken = null)
        {
            using (var webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");
                if (authToken != null)
                    webRequest.SetRequestHeader(TOKEN_HEADER_KEY, authToken);
                yield return ExecuteRequest(webRequest, body, success, failure);
            }
        }

        internal static IEnumerator Get<TR>(string url, Action<TR> success, Action failure,
            string authToken = null)
        {
            using (var webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Accept", "*/*");
                if (authToken != null)
                    webRequest.SetRequestHeader(TOKEN_HEADER_KEY, authToken);
                yield return ExecuteRequest(webRequest, success, failure);
            }
        }

        internal static IEnumerator Delete(string url, Action success, Action failure,
            string authToken = null)
        {
            using (var webRequest = UnityWebRequest.Delete(url))
            {
                webRequest.SetRequestHeader("Accept", "*/*");
                if (authToken != null)
                    webRequest.SetRequestHeader(TOKEN_HEADER_KEY, authToken);
                yield return ExecuteRequest(webRequest, success, failure);
            }
        }

        internal static IEnumerator ExecuteRequest<TB, TR>(UnityWebRequest webRequest, TB body, Action<TR> success,
            Action failure)
        {
            yield return ExecuteRequest(webRequest, body, () => success(ReadResponse<TR>(webRequest)), failure);
        }

        internal static IEnumerator ExecuteRequest<TB>(UnityWebRequest webRequest, TB body, Action success,
            Action failure)
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            var data = JsonConvert.SerializeObject(body, null, settings);
            var bodyRaw = Encoding.UTF8.GetBytes(data);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return ExecuteRequest(webRequest, success, failure);
        }

        internal static IEnumerator ExecuteRequest<TR>(UnityWebRequest webRequest, Action<TR> consumer, Action failed)
        {
            yield return ExecuteRequest(webRequest, () => consumer(ReadResponse<TR>(webRequest)), failed);
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

        private static T ReadResponse<T>(UnityWebRequest webRequest)
        {
            return webRequest.responseCode == 404
                ? default
                : JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
        }
    }
}