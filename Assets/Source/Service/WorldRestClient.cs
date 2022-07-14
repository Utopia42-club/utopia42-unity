using System;
using System.Collections;
using Source.Model;
using Source.Service.Ethereum;
using Source.Utils;
using UnityEngine.Networking;

namespace Source.Service
{
    public class WorldRestClient
    {
        public static readonly WorldRestClient INSTANCE = new WorldRestClient();

        public IEnumerator LoadNetworks(Action<EthNetwork[]> consumer, Action failed)
        {
            using (var webRequest = UnityWebRequest.Get(Constants.NetsURL))
            {
                yield return RestClient.ExecuteRequest(webRequest, consumer, failed);
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
                yield return RestClient.ExecuteRequest(webRequest, consumer, failed);
            }
        }

        public IEnumerator SetLandMetadata(LandMetadata landMetadata, Action success, Action failed)
        {
            string url = Constants.ApiURL + "/land-metadata/set";
            yield return RestClient.Post(url, landMetadata, success, failed);
        }
    }
}