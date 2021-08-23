using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Utopia.Contracts.Utopia.ContractDefinition
{
    public partial class Land : LandBase { }

    public class LandBase 
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
}
