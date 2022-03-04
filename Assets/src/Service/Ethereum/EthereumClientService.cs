using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Web3;
using src.Model;
using src.Service.Ethereum.ContractDefinition;
using Land = src.Model.Land;

namespace src.Service.Ethereum
{
    public class EthereumClientService
    {
        public readonly static EthereumClientService INSTANCE = new EthereumClientService();
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

        public IEnumerator GetLastLandId(Action<BigInteger> consumer, Action onFailed)
        {
            // consumer(6); yield break; // for test only 
            var request =
                new QueryUnityRequest<LastLandIdFunction, LastLandIdOutputDTO>(network.provider,
                    network.contractAddress);
            yield return request.Query(new LastLandIdFunction() { }, network.contractAddress);
            if (request.Result != null)
                consumer(request.Result.ReturnValue1);
            else onFailed();
        }

        public IEnumerator GetLandPrice(long x1, long x2, long y1, long y2, Action<decimal> consumer, Action onFailed)
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
            if (request.Result != null)
                consumer(Web3.Convert.FromWei(request.Result.ReturnValue1));
            else onFailed();
        }


        public IEnumerator ABS(Action<BigInteger> consumer, Action onFailed)
        {
            var request =
                new QueryUnityRequest<AbsFunction, AbsOutputDTO>(network.provider, network.contractAddress);
            yield return request.Query(new AbsFunction() {X = -22}, network.contractAddress);
            // Debug.Log(request.Exception);
            // Debug.Log(request.DefaultAccount);
            // Debug.Log(request.Result.ReturnValue1);

            if (request.Result != null)
                consumer(request.Result.ReturnValue1);
            else onFailed();
            // consumer(MapLands(request.Result.Lands));
        }

        public IEnumerator GetLandsForOwner(string owner, Action<List<Land>> consumer, Action onFailed)
        {
            var request =
                new QueryUnityRequest<GetLandsFunction, GetLandsOutputDTO>(network.provider, network.contractAddress);
            yield return request.Query(new GetLandsFunction() {Owner = owner}, network.contractAddress);
            if (request.Result != null)
                consumer(MapLands(request.Result.Lands));
            else onFailed();
        }

        public IEnumerator GetLandsByIds(List<BigInteger> ids, Action<List<Land>> consumer, Action onFailed)
        {
            var request =
                new QueryUnityRequest<GetLandsByIdsFunction, GetLandsByIdsOutputDTO>(network.provider,
                    network.contractAddress);
            //TODO add exception handling
            yield return request.Query(new GetLandsByIdsFunction() {Ids = ids}, network.contractAddress);
            if (request.Result != null)
                consumer(MapLands(request.Result.Lands));
            else onFailed();
        }

        private static List<Land> MapLands(List<ContractDefinition.Land> contractLands)
        {
            List<Land> resultLands = new List<Land>();
            if (contractLands != null)
                foreach (var contractLand in contractLands)
                {
                    var land = new Land();
                    land.id = (long) contractLand.Id;
                    land.startCoordinate = new SerializableVector3Int((int) contractLand.X1, 0, (int) contractLand.Y1);
                    land.endCoordinate = new SerializableVector3Int((int) contractLand.X2, 0, (int) contractLand.Y2);
                    land.ipfsKey = contractLand.Hash;
                    land.time = (long) contractLand.Time;
                    land.isNft = contractLand.IsNFT;
                    land.owner = contractLand.Owner.ToLower();
                    land.ownerIndex = (long) contractLand.OwnerIndex;
                    resultLands.Add(land);
                }

            return resultLands;
        }

        public IEnumerator GetLands(List<Land> resultLands, Action onFailed)
        {
            BigInteger lastId = 0;
            yield return GetLastLandId(result => lastId = result, onFailed);

            var pageSize = lastId < 50 ? (int) lastId : 50;
            var ids = new List<BigInteger>(pageSize);
            for (var i = 1; i <= pageSize; i++) ids.Add(i);

            while (true)
            {
                if (ids.Count == 0) yield break;

                var failed = false;
                yield return GetLandsByIds(ids, lands =>
                {
                    foreach (var land in lands)
                    {
                        if (land.owner == null || land.owner.Length == 0 ||
                            land.startCoordinate.x == land.endCoordinate.x
                            || land.startCoordinate.z == land.endCoordinate.z)
                            continue;
                        resultLands.Add(land);
                    }
                }, () =>
                {
                    failed = true;
                    onFailed();
                });
                if (failed) yield break;

                var currentLast = ids[ids.Count - 1];
                if (currentLast + pageSize > lastId)
                    ids = ids.GetRange(0, (int) (lastId - currentLast));
                for (var i = 0; i < ids.Count; i++) ids[i] = i + currentLast + 1;
            }
        }
    }
}