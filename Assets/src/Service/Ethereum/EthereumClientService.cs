using Nethereum.JsonRpc.UnityClient;
using System;
using System.Collections;
using System.Collections.Generic;
using Utopia.Contracts.Utopia.ContractDefinition;

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

    public IEnumerator getOwners(Action<List<string>> consumer)
    {
        var request = new QueryUnityRequest<GetOwnersFunction, GetOwnersOutputDTOBase>(network.provider, network.contractAddress);
        yield return request.Query(new GetOwnersFunction() { }, network.contractAddress);
        consumer.Invoke(request.Result.ReturnValue1);
        yield break;
    }

    public IEnumerator getLandsForOwner(string owner, Action<List<Land>> consumer)
    {
        List<Land> lands = new List<Land>();
        var request = new QueryUnityRequest<GetLandsFunction, GetLandsOutputDTO>(network.provider, network.contractAddress);
        yield return request.Query(new GetLandsFunction() { Owner = owner }, network.contractAddress);
        var results = request.Result.ReturnValue1;
        if (results != null)
            foreach (var result in results)
            {
                var land = new Land();
                land.x1 = (long)result.X1;
                land.y1 = (long)result.Y1;
                land.x2 = (long)result.X2;
                land.y2 = (long)result.Y2;
                land.ipfsKey = result.Hash;
                land.time = (long)result.Time;
                lands.Add(land);
            }
        consumer(lands);
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
