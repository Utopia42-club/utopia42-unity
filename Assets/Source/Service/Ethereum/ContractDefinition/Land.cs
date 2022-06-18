using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Source.Service.Ethereum.ContractDefinition
{
    public partial class Land : LandBase { }

    public class LandBase 
    {
        [Parameter("uint256", "id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("int256", "x1", 2)]
        public virtual BigInteger X1 { get; set; }
        [Parameter("int256", "x2", 3)]
        public virtual BigInteger X2 { get; set; }
        [Parameter("int256", "y1", 4)]
        public virtual BigInteger Y1 { get; set; }
        [Parameter("int256", "y2", 5)]
        public virtual BigInteger Y2 { get; set; }
        [Parameter("uint256", "time", 6)]
        public virtual BigInteger Time { get; set; }
        [Parameter("string", "hash", 7)]
        public virtual string Hash { get; set; }
        [Parameter("bool", "isNFT", 8)]
        public virtual bool IsNFT { get; set; }
        [Parameter("address", "owner", 9)]
        public virtual string Owner { get; set; }
        [Parameter("uint256", "ownerIndex", 10)]
        public virtual BigInteger OwnerIndex { get; set; }
    }
}
