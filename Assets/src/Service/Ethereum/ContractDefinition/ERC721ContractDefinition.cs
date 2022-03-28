using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace src.Service.Ethereum.ContractDefinition
{
    public partial class TokenUriFunction : TokenUriFunctionBase
    {
    }

    [Function("tokenURI", "string")]
    public class TokenUriFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_tokenId", 1)] public virtual BigInteger TokenId { get; set; }
    }

    public partial class TokenUriOutputDto : TokenUriOutputDtoBase
    {
    }

    [FunctionOutput]
    public class TokenUriOutputDtoBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)] public virtual string ReturnValue1 { get; set; }
    }
}