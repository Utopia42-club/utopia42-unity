using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.JsonRpc.UnityClient;
using src.Service.Ethereum.ContractDefinition;
using UnityEngine;
using Land = src.Model.Land;

namespace src.Service.Ethereum
{
    public class EthereumClientService
    {
        public static EthereumClientService INSTANCE = new EthereumClientService();
        private EthNetwork network;

        private EthereumClientService()
        {
        }

        public bool IsInited()
        {
            return network != null;
        }

        public EthNetwork GetNetwork()
        {
            return network;
        }

        public void SetNetwork(EthNetwork network)
        {
            this.network = network;
        }

        public IEnumerator GetLastLandId(Action<BigInteger> consumer)
        {
            var request =
                new QueryUnityRequest<LastLandIdFunction, LastLandIdOutputDTO>(network.provider,
                    network.contractAddress);
            yield return request.Query(new LastLandIdFunction() { }, network.contractAddress);
            consumer(request.Result.ReturnValue1);
        }

        public IEnumerator GetLandPrice(long x1, long x2, long y1, long y2, Action<decimal> consumer)
        {
            var request =
                new QueryUnityRequest<LandPriceFunction, LandPriceOutputDTO>(network.provider, network.contractAddress);
            yield return request.Query(new LandPriceFunction()
            {
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2
            }, network.contractAddress);
            consumer(Nethereum.Web3.Web3.Convert.FromWei(request.Result.ReturnValue1));
        }


        public IEnumerator ABS(Action<BigInteger> consumer)
        {
            var request =
                new QueryUnityRequest<AbsFunction, AbsOutputDTO>(network.provider, network.contractAddress);
            yield return request.Query(new AbsFunction() {X = -22}, network.contractAddress);
            // Debug.Log(request.Exception);
            // Debug.Log(request.DefaultAccount);
            // Debug.Log(request.Result.ReturnValue1);
            consumer(request.Result.ReturnValue1);
            // consumer(MapLands(request.Result.Lands));
        }

        public IEnumerator GetLandsForOwner(string owner, Action<List<Land>> consumer)
        {
            var request =
                new QueryUnityRequest<GetLandsFunction, GetLandsOutputDTO>(network.provider, network.contractAddress);
            yield return request.Query(new GetLandsFunction() {Owner = owner}, network.contractAddress);
            Debug.Log(request.Exception);
            Debug.Log(request.DefaultAccount);
            Debug.Log(request.Result.Lands);
            Debug.Log(request.Result.Lands.Count);
            consumer(MapLands(request.Result.Lands));
        }

        public IEnumerator GetLandsByIds(List<BigInteger> ids, Action<List<Land>> consumer)
        {
            var request =
                new QueryUnityRequest<GetLandsByIdsFunction, GetLandsByIdsOutputDTO>(network.provider,
                    network.contractAddress);
            yield return request.Query(new GetLandsByIdsFunction() {Ids = ids}, network.contractAddress);
            consumer(MapLands(request.Result.Lands));
        }

        private static List<Land> MapLands(List<src.Service.Ethereum.ContractDefinition.Land> contractLands)
        {
            List<Land> resultLands = new List<Land>();
            if (contractLands != null)
                foreach (var contractLand in contractLands)
                {
                    var land = new Land();
                    land.id = (long) contractLand.Id;
                    land.x1 = (long) contractLand.X1;
                    land.y1 = (long) contractLand.Y1;
                    land.x2 = (long) contractLand.X2;
                    land.y2 = (long) contractLand.Y2;
                    land.ipfsKey = contractLand.Hash;
                    land.time = (long) contractLand.Time;
                    land.isNft = contractLand.IsNFT;
                    land.owner = contractLand.Owner.ToLower();
                    land.ownerIndex = (long) contractLand.OwnerIndex;
                    resultLands.Add(land);
                }

            return resultLands;
        }

        public IEnumerator GetLands(Dictionary<string, List<Land>> ownersLands)
        {
            BigInteger lastId = 0;
            yield return GetLastLandId(result => lastId = result);

            var pageSize = (lastId + 1) < 50 ? (int) lastId + 1 : 50;
            var ids = new List<BigInteger>(pageSize);
            for (var i = 1; i < pageSize; i++) ids.Add(i);

            while (true)
            {
                if (ids.Count == 0) yield break;
                yield return GetLandsByIds(ids, lands =>
                {
                    foreach (var land in lands)
                    {
                        if (land.owner == null || land.owner.Length == 0)
                            continue;
                        List<Land> ol;
                        if (!ownersLands.TryGetValue(land.owner, out ol))
                            ownersLands[land.owner] = ol = new List<Land>();
                        ol.Add(land);
                    }
                });
                var cl = ids[ids.Count - 1];
                if (cl + pageSize > lastId)
                    ids = ids.GetRange(0, (int) (lastId - cl));
                for (var i = 1; i <= ids.Count; i++) ids[i] = i + cl;
            }
        }
    }
}