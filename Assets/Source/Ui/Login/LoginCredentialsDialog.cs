using System.Linq;
using Source.Service.Ethereum;
using UnityEngine.UIElements;

namespace Source.Ui.Login
{
    public class LoginCredentialsDialog : UxmlElement
    {
        private readonly TextField walletField;
        private readonly DropdownField networkField;

        public LoginCredentialsDialog() : base(typeof(LoginCredentialsDialog))
        {
            walletField = this.Q<TextField>("walletField");
            networkField = this.Q<DropdownField>("networkField");
            var nets = EthNetwork.GetNetworksIfPresent();
            networkField.choices = nets.Select(network => network.name).ToList();
            ResetInputs();
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

        public bool AreInputsValid()
        {
            return networkField.index >= 0 && !string.IsNullOrEmpty(walletField.text);
        }
    }
}