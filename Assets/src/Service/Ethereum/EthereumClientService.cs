using Nethereum.JsonRpc.UnityClient;
using Smart_contracts.Contracts.Utopia.ContractDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class EthereumClientService
{
    public static EthereumClientService INSTANCE = new EthereumClientService("https://mainnet.infura.io/v3/b12c1b1e6b2e4f58af559a67fe46104e"
        , "0x56040d44f407fa6f33056d4f352d2e919a0d99fb");

    private readonly string url;
    private readonly string contract;
    private EthereumClientService(string url, string contract)
    {
        this.url = url;
        this.contract = contract;
    }

    public IEnumerator getOwners(Action<List<string>> consumer)
    {
        var request = new QueryUnityRequest<GetOwnersFunction, GetOwnersOutputDTOBase>(url, contract);
        yield return request.Query(new GetOwnersFunction() { }, contract);
        consumer.Invoke(request.Result.ReturnValue1);
        yield break;
    }

    IEnumerator getLandsForOwner(string owner, Action<List<Land>> consumer)
    {
        var lands = new List<Land>();
        BigInteger index = 0;
        while (true)
        {
            var request = new QueryUnityRequest<GetLandFunction, GetLandOutputDTO>(url, contract);
            yield return request.Query(new GetLandFunction() { Owner = owner, Index = index }, contract);
            index++;
            var result = request.Result;
            if (result == null || result.Time <= 0)
            {
                consumer.Invoke(lands);
                yield break;
            }
            var land = new Land();
            land.x1 = result.X1;
            land.y1 = result.Y1;
            land.x2 = result.X2;
            land.y2 = result.Y2;
            land.ipfsKey = result.Hash;
            land.time = result.Time;
            lands.Add(land);
        }
    }

    public IEnumerator getLands(Dictionary<string, List<Land>> ownersLands)
    {
        var owners = new List<string>();
        yield return getOwners(o => owners.AddRange(o));

        IEnumerator[] enums = new IEnumerator[owners.Count];
        for (int i = 0; i < owners.Count; i++)
        {
            int idx = i;
            enums[i] = getLandsForOwner(owners[i], ls => ownersLands[owners[idx]] = ls);
        }

        List<Land> lands = new List<Land>();
        for (int i = 0; i < owners.Count; i++)
            yield return enums[i];

        yield break;
    }
}
