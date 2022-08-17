using System;
using UnityEngine;

namespace Source.Service.Auth
{
    [Serializable]
    public class Session
    {
        internal static readonly string GUEST_WALLET = "guest";
        private readonly int network;
        private readonly string contract;
        private readonly string walletId;
        public string WalletId => walletId;
        public bool IsGuest => walletId.Equals(GUEST_WALLET);
        public string Contract => contract;
        public int Network => network;

        public Session(int network, string contract, string walletId)
        {
            this.network = network;
            this.contract = contract?.ToLower();
            this.walletId = walletId?.ToLower() ?? GUEST_WALLET;
        }

        internal bool HasNetwork()
        {
            return network > 0;
        }


        internal void Save()
        {
            PlayerPrefs.SetInt(Keys.NETWORK, network);
            PlayerPrefs.SetString(Keys.CONTRACT, contract);
            PlayerPrefs.SetString(Keys.WALLET, walletId);
        }

        internal static Session Load()
        {
            return new Session(PlayerPrefs.GetInt(Keys.NETWORK, -1), PlayerPrefs.GetString(Keys.CONTRACT),
                PlayerPrefs.GetString(Keys.WALLET));
        }

        private static class Keys
        {
            internal static readonly string WALLET = "WALLET";
            internal static readonly string NETWORK = "NETWORK";
            internal static readonly string CONTRACT = "CONTRACT";
        }
    }
}