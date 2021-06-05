using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Collections.Generic;
using System.Numerics;

namespace Smart_contracts.Contracts.Utopia.ContractDefinition
{


    public partial class UtopiaDeployment : UtopiaDeploymentBase
    {
        public UtopiaDeployment() : base(BYTECODE) { }
        public UtopiaDeployment(string byteCode) : base(byteCode) { }
    }

    public class UtopiaDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "608060405234801561001057600080fd5b50610470806100206000396000f3fe608060405234801561001057600080fd5b50600436106100575760003560e01c8063025e7c271461005c578063254c44f51461008c57806365742555146100ad578063756efcf7146100e1578063a0e67e2b146100f4575b600080fd5b61006f61006a366004610274565b610103565b6040516001600160a01b0390911681526020015b60405180910390f35b6100a061009a36600461022a565b50606090565b6040516100839190610324565b6100cf6100bb36600461024b565b600080600080600060609295509295509295565b604051610083969594939291906103c2565b6100cf6100ef36600461024b565b61012d565b606060405161008391906102d7565b6000818154811061011357600080fd5b6000918252602090912001546001600160a01b0316905081565b6001602052816000526040600020818154811061014957600080fd5b90600052602060002090600602016000915091505080600001549080600101549080600201549080600301549080600401549080600501805461018b906103ff565b80601f01602080910402602001604051908101604052809291908181526020018280546101b7906103ff565b80156102045780601f106101d957610100808354040283529160200191610204565b820191906000526020600020905b8154815290600101906020018083116101e757829003601f168201915b5050505050905086565b80356001600160a01b038116811461022557600080fd5b919050565b60006020828403121561023b578081fd5b6102448261020e565b9392505050565b6000806040838503121561025d578081fd5b6102668361020e565b946020939093013593505050565b600060208284031215610285578081fd5b5035919050565b60008151808452815b818110156102b157602081850181015186830182015201610295565b818111156102c25782602083870101525b50601f01601f19169290920160200192915050565b6020808252825182820181905260009190848201906040850190845b818110156103185783516001600160a01b0316835292840192918401916001016102f3565b50909695505050505050565b60006020808301818452808551808352604092508286019150828160051b870101848801865b838110156103b457888303603f1901855281518051845287810151888501528681015187850152606080820151908501526080808201519085015260a09081015160c0918501829052906103a08186018361028c565b96890196945050509086019060010161034a565b509098975050505050505050565b86815285602082015284604082015283606082015282608082015260c060a082015260006103f360c083018461028c565b98975050505050505050565b600181811c9082168061041357607f821691505b6020821081141561043457634e487b7160e01b600052602260045260246000fd5b5091905056fea264697066735822122095742eb4e4746d2cda09fccd1552cbe4ce9cb6e4479a8e924d1885f71275e91664736f6c63430008040033";
        public UtopiaDeploymentBase() : base(BYTECODE) { }
        public UtopiaDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class GetLandFunction : GetLandFunctionBase { }

    [Function("getLand", typeof(GetLandOutputDTO))]
    public class GetLandFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
        [Parameter("uint256", "index", 2)]
        public virtual BigInteger Index { get; set; }
    }

    public partial class GetLandsFunction : GetLandsFunctionBase { }

    [Function("getLands", typeof(GetLandsOutputDTO))]
    public class GetLandsFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
    }

    public partial class GetOwnersFunction : GetOwnersFunctionBase { }

    [Function("getOwners", "address[]")]
    public class GetOwnersFunctionBase : FunctionMessage
    {

    }

    public partial class LandsFunction : LandsFunctionBase { }

    [Function("lands", typeof(LandsOutputDTO))]
    public class LandsFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
        [Parameter("uint256", "", 2)]
        public virtual BigInteger ReturnValue2 { get; set; }
    }

    public partial class OwnersFunction : OwnersFunctionBase { }

    [Function("owners", "address")]
    public class OwnersFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetLandOutputDTO : GetLandOutputDTOBase { }

    [FunctionOutput]
    public class GetLandOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("int256", "x1", 1)]
        public virtual BigInteger X1 { get; set; }
        [Parameter("int256", "y1", 2)]
        public virtual BigInteger Y1 { get; set; }
        [Parameter("int256", "x2", 3)]
        public virtual BigInteger X2 { get; set; }
        [Parameter("int256", "y2", 4)]
        public virtual BigInteger Y2 { get; set; }
        [Parameter("uint256", "time", 5)]
        public virtual BigInteger Time { get; set; }
        [Parameter("string", "hash", 6)]
        public virtual string Hash { get; set; }
    }

    public partial class GetLandsOutputDTO : GetLandsOutputDTOBase { }

    [FunctionOutput]
    public class GetLandsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("tuple[]", "", 1)]
        public virtual List<Land> ReturnValue1 { get; set; }
    }

    public partial class GetOwnersOutputDTO : GetOwnersOutputDTOBase { }

    [FunctionOutput]
    public class GetOwnersOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address[]", "", 1)]
        public virtual List<string> ReturnValue1 { get; set; }
    }

    public partial class LandsOutputDTO : LandsOutputDTOBase { }

    [FunctionOutput]
    public class LandsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("int256", "x1", 1)]
        public virtual BigInteger X1 { get; set; }
        [Parameter("int256", "x2", 2)]
        public virtual BigInteger X2 { get; set; }
        [Parameter("int256", "y1", 3)]
        public virtual BigInteger Y1 { get; set; }
        [Parameter("int256", "y2", 4)]
        public virtual BigInteger Y2 { get; set; }
        [Parameter("uint256", "time", 5)]
        public virtual BigInteger Time { get; set; }
        [Parameter("string", "hash", 6)]
        public virtual string Hash { get; set; }
    }

    public partial class OwnersOutputDTO : OwnersOutputDTOBase { }

    [FunctionOutput]
    public class OwnersOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }
}
