using System;
using System.Linq;
using Source.Model;
using Source.Service.Ethereum;
using UnityEngine;
using UnityEngine.Events;

namespace Source
{
    public class AuthService
    {
        private static readonly string GUEST = "guest";
        public static readonly UnityEvent<string> WalletIdChanged = new();

        public static void GetAuthToken(Action<string> onDone, bool forceValid = false)
        {
            if (!WebBridge.IsPresent())
            {
                onDone(null);
                return; //FIXME
            }

            WebBridge.CallAsync<string>("getAuthToken", forceValid, onDone.Invoke);
        }

        public static void Connect(Action<ConnectionDetail> onDone)
        {
            var nets = EthNetwork.GetNetworksIfPresent();
            if (nets == null || nets.Length == 0)
                throw new Exception("Networks are not loaded");
            WebBridge.CallAsync<ConnectionDetail>("connectMetamask", nets.First().id, (ci) =>
            {
                if (ci.network.HasValue && ci.wallet != null)
                    Save(ci.network.Value, ci.wallet);
                onDone(ci);
            });
        }

        public static bool IsGuest()
        {
            return WalletId().Equals(GUEST);
        }

        public static string WalletId()
        {
            return PlayerPrefs.GetString(Keys.WALLET)?.ToLower();
        }

        public static EthNetwork Network()
        {
            var nets = EthNetwork.GetNetworksIfPresent();
            return EthNetwork.GetByIdIfPresent(PlayerPrefs.GetInt(Keys.NETWORK,
                nets == null || nets.Length == 0 ? -1 : nets[0].id));
        }

        public static ConnectionDetail ConnectionDetail()
        {
            var detail = new ConnectionDetail
            {
                wallet = WalletId()
            };
            var net = Network();
            detail.network = net?.id ?? -1;
            return detail;
        }

        public static void Save(int network, string walletId)
        {
            PlayerPrefs.SetInt(Keys.NETWORK, network);
            PlayerPrefs.SetString(Keys.WALLET, walletId);
            WalletIdChanged.Invoke(walletId);
        }

        public static void SetGuestMode()
        {
            Save(-1, GUEST);
        }

        private class Keys
        {
            public static readonly string WALLET = "WALLET";
            public static readonly string NETWORK = "NETWORK";
        }
    }
}