using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using src.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace src.Service
{
    public class IpfsClient
    {
        private static readonly string SERVER_URL = "https://utopia42.club/api/v0";

        public static IpfsClient INSATANCE = new IpfsClient();

        private IpfsClient()
        {
        }

        public IEnumerator GetLandDetails(Land land, Action<LandDetails> consumer)
        {
            string url = SERVER_URL + "/cat?arg=/ipfs/" + land.ipfsKey;
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
                        details.region = land;
                        consumer.Invoke(details);
                        break;
                }
            }
        }

        public IEnumerator Upload(List<LandDetails> details, Action<Dictionary<long, string>> success)
        {
            var result = new Dictionary<long, string>();
            string url = SERVER_URL + "/add?stream-channels=true&progress=false";
            foreach (var landDetail in details)
            {
                if (string.IsNullOrWhiteSpace(landDetail.region.ipfsKey) && landDetail.changes.Count == 0)
                    continue;

                string data = JsonConvert.SerializeObject(landDetail);
                var form = new List<IMultipartFormSection>();
                form.Add(new MultipartFormDataSection("file", data));
                using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
                {
                    yield return webRequest.SendWebRequest();

                    switch (webRequest.result) //FIXME add proper error handling
                    {
                        case UnityWebRequest.Result.ConnectionError:
                        case UnityWebRequest.Result.DataProcessingError:
                            Debug.LogError(string.Format("Posing data for {0} caused Error: {1}", url, webRequest.error));
                            //result.Add(detail.region.ipfsKey);
                            break;
                        case UnityWebRequest.Result.ProtocolError:
                            Debug.LogError(string.Format("Get for {0} caused HTTP Error: {1}", url, webRequest.error));
                            //result.Add(detail.region.ipfsKey);
                            break;
                        case UnityWebRequest.Result.Success:
                            var response = JsonConvert.DeserializeObject<IpfsResponse>(webRequest.downloadHandler.text);
                            result[landDetail.region.id] = response.hash;
                            break;
                    }
                }
            }

            success.Invoke(result);
            yield break;
        }
    }


    [Serializable]
    class IpfsResponse
    {
        public string name;
        public string hash;
        public string size;
    }
}