using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace Smart_contracts.Contracts.Utopia.ContractDefinition
{


    public partial class UtopiaDeployment : UtopiaDeploymentBase
    {
        public UtopiaDeployment() : base(BYTECODE) { }
        public UtopiaDeployment(string byteCode) : base(byteCode) { }
    }

    public class UtopiaDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "608060405234801561001057600080fd5b506101a7806100206000396000f3fe608060405234801561001057600080fd5b50600436106100365760003560e01c8063025e7c271461003b578063a0e67e2b1461006b575b600080fd5b61004e61004936600461010c565b610080565b6040516001600160a01b0390911681526020015b60405180910390f35b6100736100aa565b6040516100629190610124565b6000818154811061009057600080fd5b6000918252602090912001546001600160a01b0316905081565b6060600080548060200260200160405190810160405280929190818152602001828054801561010257602002820191906000526020600020905b81546001600160a01b031681526001909101906020018083116100e4575b5050505050905090565b60006020828403121561011d578081fd5b5035919050565b6020808252825182820181905260009190848201906040850190845b818110156101655783516001600160a01b031683529284019291840191600101610140565b5090969550505050505056fea26469706673582212209192109aaf8fb5af93f00a85d32e188c9df55f0a4be67d425ec421dd2348867964736f6c63430008040033";
        public UtopiaDeploymentBase() : base(BYTECODE) { }
        public UtopiaDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class GetOwnersFunction : GetOwnersFunctionBase { }

    [Function("getOwners", "address[]")]
    public class GetOwnersFunctionBase : FunctionMessage
    {

    }

    public partial class OwnersFunction : OwnersFunctionBase { }

    [Function("owners", "address")]
    public class OwnersFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetOwnersOutputDTO : GetOwnersOutputDTOBase { }

    [FunctionOutput]
    public class GetOwnersOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address[]", "", 1)]
        public virtual List<string> ReturnValue1 { get; set; }
    }

    public partial class OwnersOutputDTO : OwnersOutputDTOBase { }

    [FunctionOutput]
    public class OwnersOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }
}
