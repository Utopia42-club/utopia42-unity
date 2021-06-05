using Nethereum.JsonRpc.UnityClient;
using Smart_contracts.Contracts.Utopia.ContractDefinition;
using System.Collections.Generic;

public class EthereumClientService
{
    private static string NETWORK_URL = "https://mainnet.infura.io/v3/b12c1b1e6b2e4f58af559a67fe46104e";
    private static string CONTRACT_ADDRESS = "0x56040d44f407fa6f33056d4f352d2e919a0d99fb";

    public List<string> getOwners()
    {
        var queryRequest = new QueryUnityRequest<GetOwnersFunction, GetOwnersOutputDTOBase>(NETWORK_URL, CONTRACT_ADDRESS);
        queryRequest.Query(new GetOwnersFunction() { }, CONTRACT_ADDRESS);
        return queryRequest.Result.ReturnValue1;
    }
}
