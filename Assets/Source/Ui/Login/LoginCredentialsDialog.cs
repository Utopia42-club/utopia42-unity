using System.Linq;
using Source.Service.Ethereum;
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
            var nets = EthNetwork.GetNetworksIfPresent();
            networkField.choices = nets.Select(network => network.name).ToList();
            ResetInputs();
        }

        private void ResetInputs()
        {
            walletField.value = Login.IsGuest() ? null : Login.WalletId();

            var net = Login.Network();
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
            PlayerPrefs.SetInt(Keys.NETWORK, EthNetwork.GetNetworksIfPresent()[networkField.index].id);
            PlayerPrefs.SetString(Keys.WALLET, walletField.text);
        }
    }
}