﻿using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Source.Configuration;
using UnityEngine.Networking;

namespace Source.Service
{
    internal class IpfsClient
    {
        internal static IpfsClient INSATANCE = new();

        private IpfsClient()
        {
        }

        public static string ToPublicGatewayUrl(string key)
        {
            return $"{Configurations.Instance.publicIpfsGateway}/ipfs/{key}";
        }
        
        public static string ToUtopiaIpfsServerUrl(string key)
        {
            return Configurations.Instance.ipfsServerURL + "/cat?arg=/ipfs/" + key;
        }

        public IEnumerator DownloadJson<TR>(string key, Action<TR> onSuccess, Action onFailure)
        {
            yield return RestClient.Get(ToUtopiaIpfsServerUrl(key), onSuccess, onFailure);
        }

        public IEnumerator UploadJson<TB>(TB body, Action<string> onSuccess, Action onFailure)
        {
            var form = new List<IMultipartFormSection>
                {new MultipartFormDataSection("file", JsonConvert.SerializeObject(body))};
            yield return Upload(form, onSuccess, onFailure);
        }

        public IEnumerator UploadImage(byte[] image, Action<string> onSuccess, Action onFailure)
        {
            var form = new List<IMultipartFormSection> {new MultipartFormDataSection("image", image, "image/png")};
            yield return Upload(form, onSuccess, onFailure);
        }

        private static IEnumerator Upload(List<IMultipartFormSection> form, Action<string> onSuccess,
            Action onFailure)
        {
            var url = Configurations.Instance.ipfsServerURL + "/add?stream-channels=true&progress=false";
            using (var webRequest = UnityWebRequest.Post(url, form))
            {
                yield return RestClient.ExecuteRequest<IpfsResponse>(webRequest,
                    ipfsResponse => onSuccess.Invoke(ipfsResponse.hash),
                    onFailure);
            }
        }

        /**
         * returns true if key is a valid CIDv0 CID (46 char long and starts with Qm)
         */
        public static bool IsKeyValid(String key)
        {
            return key != null &&
                   key.Length == 46 && key.StartsWith("Qm");
        }

        [Serializable]
        class IpfsResponse
        {
            public string name;
            public string hash;
            public string size;
        }
    }
}