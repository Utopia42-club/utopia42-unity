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
            var land = new Land((int)result.X1, (int)result.Y1, (int)result.X2, (int)result.Y2, (int)result.Time, result.Hash);
            lands.Add(land);
        }
    }

    public IEnumerator getLands(Action<List<Land>> consumer)
    {
        var owners = new List<string>();
        yield return getOwners(o => owners.AddRange(o));

        var lands = new List<Land>();
        foreach (var owner in owners)
        {
            yield return getLandsForOwner(owner, ls => lands.AddRange(ls));
        }
        consumer.Invoke(lands);
        yield break;
    }
}
