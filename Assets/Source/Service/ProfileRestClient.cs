using System;
using System.Collections;
using Source.Configuration;
using Source.Model;
using Source.Utils;
using UnityEngine.Networking;

namespace Source.Service
{
    public class ProfileRestClient
    {
        public static readonly ProfileRestClient INSTANCE = new();
        private string baseUrl => $"{Configurations.Instance.apiURL}/profile";

        public IEnumerator GetProfile(string walletId, Action<Profile> consumer, Action failed)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Post(baseUrl, walletId))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");
                yield return RestClient.ExecuteRequest(webRequest, consumer, failed);
            }
        }

        public string GetImageUrl(string imageUrl)
        {
            return $"{baseUrl}/image/{imageUrl}";
        }
    }
}