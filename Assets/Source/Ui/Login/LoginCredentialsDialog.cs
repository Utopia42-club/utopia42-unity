using System;
using System.Linq;
using Source.Service.Ethereum;
using Source.Ui.Snack;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Login
{
    public class LoginCredentialsDialog : UxmlElement
    {
        private readonly TextField walletField;
        private readonly DropdownField networkField;

        public LoginCredentialsDialog() : base("Ui/Login/LoginCredentialsDialog")
        {
            walletField = this.Q<TextField>("walletField");
            networkField = this.Q<DropdownField>("networkField");
            var loading = LoadingLayer.LoadingLayer.Show(this);
            GameManager.INSTANCE.StartCoroutine(EthNetwork.GetNetworks(_ =>
                {
                    loading.Close();
                    var nets = EthNetwork.GetNetworksIfPresent();
                    networkField.choices = nets.Select(network => network.name).ToList();
                    ResetInputs();
                },
                () =>
                {
                    SnackService.INSTANCE.Show(
                        new SnackConfig(
                            new Toast("Could not load any ETHEREUM networks. Please report the error.",
                                Toast.ToastType.Error)
                        )
                    );
                    loading.Close();
                }));
        }

        private void ResetInputs()
        {
            walletField.value = AuthService.IsGuest() ? null : AuthService.WalletId();

            var net = AuthService.Network();
            networkField.value = null;

            var nets = EthNetwork.GetNetworksIfPresent();
            for (int i = 0; i < networkField.choices.Count; i++)
            {
                if (nets[i].Equals(net))
                {
                    networkField.index = i;
                    break;
                }
            }
        }

        public void SaveInputs()
        {
            AuthService.Save(EthNetwork.GetNetworksIfPresent()[networkField.index].id, walletField.text);
        }
    }
}