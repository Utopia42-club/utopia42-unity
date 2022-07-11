using Source.Service.Auth;
using UnityEngine.UIElements;

namespace Source.Ui.Login
{
    public class LoginCredentialsDialog : UxmlElement
    {
        private readonly TextField walletField;

        public LoginCredentialsDialog() : base(typeof(LoginCredentialsDialog))
        {
            walletField = this.Q<TextField>("walletField");

            var savedSession = Session.Load();
            walletField.value = savedSession.IsGuest ? null : savedSession.WalletId;
        }

        public string GetWallet()
        {
            return walletField.text;
        }

        public bool AreInputsValid()
        {
            return !string.IsNullOrEmpty(walletField.text);
        }
    }
}