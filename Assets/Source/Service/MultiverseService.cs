using System;
using System.Collections;
using System.Collections.Generic;
using Source.Configuration;
using Source.Model;
using Source.Utils.Tasks;

namespace Source.Service
{
    public class MultiverseService
    {
        public static readonly MultiverseService Instance = new();
        private string baseUrl => Configurations.Instance.apiURL + "/world";
        private readonly CachingTask<List<NetworkData>> networksTask;

        private MultiverseService()
        {
            networksTask =
                new CachingTask<List<NetworkData>>(l =>
                    RestClient.Get($"{baseUrl}/networks", l.onSuccess, l.onFailure));
        }

        public IEnumerator GetDefaultContract(Action<MetaverseContract> success, Action failure)
        {
            yield return RestClient.Get($"{baseUrl}/contracts/default", success, failure);
        }

        public IEnumerator GetContract(int networkId, string contract, Action<MetaverseContract> success,
            Action failure)
        {
            yield return RestClient.Get($"{baseUrl}/contracts/{networkId}/{contract}", success, failure);
        }

        public IEnumerator GetAllNetworks(Action<List<NetworkData>> success, Action failure)
        {
            yield return networksTask.Get(success, failure);
        }

        public IEnumerator GetContracts(int network, int pageSize, string filter, Action<List<MetaverseContract>> success,
            Action failure)
        {
            yield return GetContracts(network, pageSize, filter, null, success, failure);
        }

        public IEnumerator GetContracts(int network, int pageSize, string filter, string lastId,
            Action<List<MetaverseContract>> success,
            Action failure)
        {
            string p = "";
            if (filter != null)
                p += $"&filter={filter}";
            if (lastId != null)
                p += $"&lastId={lastId}";

            yield return RestClient.Get($"{baseUrl}/contracts/{network}?pageSize={pageSize}{p}", success, failure);
        }
    }
}