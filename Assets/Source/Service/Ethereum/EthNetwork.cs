using System;
using System.Collections;
using UnityEngine;

namespace Source.Service.Ethereum
{
    [Serializable]
    public class EthNetwork
    {
        private static EthNetwork[] NETWORKS;
        //     = new EthNetwork[]
        // {
        //new EthNetwork(1, "0x56040d44f407fa6f33056d4f352d2e919a0d99fb", "Ethereum Main Network", "mainnet"),
        //new EthNetwork(3, "0x9344CdEc9cf176E3162758D23d1FC806a0AE08cf", "Ropsten Test Network", "ropsten"),
        //new EthNetwork(4, "0x801fC75707BEB6d2aE8863D7A3B66047A705ffc0", "Rinkeby Test Network", "rinkeby"),
        //new EthNetwork(97, "0x044630826A56C768D3FAC17f907EA38aE90BE2B3", "Binance Smart Chain Test", "bsctest", "https://data-seed-prebsc-1-s1.binance.org:8545")
        //new EthNetwork(97, "0xf4315D6Ab7B18A5903d22f9dc17358C7bBA85b8F", "Binance Smart Chain Test", "bsctest", "https://data-seed-prebsc-1-s2.binance.org:8545")
        // new EthNetwork(97, "0x4f2BF5efbefa41506c5D1Ebbc0CBFe74461A1c74", "Binance Smart Chain Test", "bsctest", "https://data-seed-prebsc-1-s3.binance.org:8545")
        // new EthNetwork(97, "0xCeA573896B3963Ee0c4cDA48dD542EFcbff009Da", "Binance Smart Chain Test", "bsctest",
        //     "https://data-seed-prebsc-2-s3.binance.org:8545")
        // };

        public int id;
        public string contractAddress;
        public string name;
        public string subdomain;
        public string provider;

        // private EthNetwork(int id, string contractAddress, string name, string subdomain)
        //     : this(id, contractAddress, name, subdomain,
        //         string.Format("https://{0}.infura.io/v3/b12c1b1e6b2e4f58af559a67fe46104e", subdomain))
        // {
        // }
        //
        // private EthNetwork(int id, string contractAddress, string name, string subdomain, string provider)
        // {
        //     this.id = id;
        //     this.contractAddress = contractAddress;
        //     this.name = name;
        //     this.subdomain = subdomain;
        //     this.provider = provider;
        // }


        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !GetType().Equals(obj.GetType()))
                return false;

            return id == ((EthNetwork) obj).id;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public static IEnumerator GetById(int id, Action<EthNetwork> consumer, Action failed)
        {
            yield return GetNetworks(nets =>
            {
                foreach (var net in nets)
                    if (net.id == id)
                        consumer(net);
                consumer(null);
            }, failed);
        }

        public static IEnumerator GetNetworks(Action<EthNetwork[]> consumer, Action failed)
        {
            if (NETWORKS == null)
            {
                yield return WorldRestClient.INSTANCE.LoadNetworks(nets =>
                {
                    if (nets == null)
                        NETWORKS = new EthNetwork[0];
                    else NETWORKS = nets;
                }, failed);
            }

            if (NETWORKS != null)
            {
                // foreach (var net in NETWORKS) // for test only
                // {
                //     net.provider = "https://rpc-mainnet.matic.quiknode.pro/";
                // }
                consumer(NETWORKS);
            }
        }

        public static EthNetwork GetByIdIfPresent(int id)
        {
            if (NETWORKS == null) return null;

            foreach (var net in NETWORKS)
                if (net.id == id)
                    return net;
            return null;
        }

        public static EthNetwork[] GetNetworksIfPresent()
        {
            return NETWORKS;
        }
    }
}