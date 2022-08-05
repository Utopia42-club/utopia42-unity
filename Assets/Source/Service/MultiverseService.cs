using System;
using System.Collections;
using System.Collections.Generic;
using Source.Configuration;
using Source.Model;
using Source.Utils;
using Source.Utils.Tasks;

namespace Source.Service
{
    public class MultiverseService
    {
        public static readonly MultiverseService Instance = new();
        private string baseUrl => Configurations.Instance.apiURL + "/world";
        private readonly CachingTask<List<MetaverseNetwork>> networksTask;

        private MultiverseService()
        {
            networksTask =
                new CachingTask<List<MetaverseNetwork>>(l => RestClient.Get($"{baseUrl}/contracts", l.onSuccess, l.onFailure));
        }

        public IEnumerator GetDefaultContract(Action<MetaverseContract> success, Action failure)
        {
            yield return RestClient.Get($"{baseUrl}/contracts/default", success, failure);
        }
        
        public IEnumerator GetContract(int networkId, string contract, Action<MetaverseContract> success, Action failure)
        {
            yield return RestClient.Get($"{baseUrl}/contracts/{networkId}/{contract}", success, failure);
        }

        public IEnumerator GetAllNetworks(Action<List<MetaverseNetwork>> success, Action failure)
        {
            yield return networksTask.Get(success, failure);
        }
    }
}