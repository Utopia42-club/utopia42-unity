using System;
using Source.Canvas;
using Source.Model;
using Source.Ui.Dialog;
using Source.Ui.LoadingLayer;
using Source.Ui.Login;
using Source.Ui.Snack;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Source.Service.Auth
{
    public class AuthService
    {
        public static readonly AuthService Instance = new();
        public string lastWallet = null;
        public readonly UnityEvent<string> walletIdChanged = new();
        public MetaverseContract CurrentContract { get; private set; }
        private Session session;

        private AuthService()
        {
        }

        public void GetAuthToken(Action<string> onDone, bool forceValid = false)
        {
            if (!WebBridge.IsPresent())
            {
                onDone(null);
                return; //FIXME
            }

            WebBridge.CallAsync<string>("getAuthToken", forceValid, onDone.Invoke);
        }

        private void ConnectProvider()
        {
            var metamaskLoading = LoadingLayer.Show();

            WebBridge.CallAsync<ConnectionDetail>("connectMetamask", null, (cd) =>
            {
                metamaskLoading.Close();
                if (cd.wallet != null)
                    SetupSessionFroWallet(cd.wallet);
                else OpenCredentialsDialog();
            });
        }

        public bool HasSession()
        {
            return session != null;
        }

        public bool IsGuest()
        {
            return session.IsGuest;
        }

        public string WalletId()
        {
            return session.WalletId;
        }

        public bool IsCurrentUser(string walletId)
        {
            return walletId != null && walletId.ToLower().Equals(WalletId());
        }

        public ConnectionDetail ConnectionDetail()
        {
            var detail = new ConnectionDetail
            {
                wallet = WalletId(),
                network = session?.Network ?? -1,
                contractAddress = CurrentContract?.address
            };
            return detail;
        }

        private void SetSession(Session s)
        {
            if (!string.Equals(s.WalletId, lastWallet, StringComparison.OrdinalIgnoreCase))
            {
                lastWallet = s.WalletId;
                walletIdChanged.Invoke(s.WalletId);
            }

            session = s;
            session.Save();
            ChangeContract(s.Network, s.Contract);
        }

        internal void ChangeContract(int network, string contract,
            Vector3? startingPosition = null)
        {
            if (contract == null || CurrentContract == null || CurrentContract.networkId != network
                || !Equals(CurrentContract.address, contract))
            {
                var loading = LoadingLayer.Show();
                Action<MetaverseContract> success = (c) =>
                {
                    loading.Close();
                    if (c == null)
                        OpenContractNotFoundDialog();
                    else
                    {
                        CurrentContract = c;
                        session = new Session(c.networkId, c.address, session.WalletId);
                        session.Save();
                        EnterMetaverse(null);
                    }
                };
                Action failure = () =>
                {
                    loading.Close();
                    new Toast("Failed to connect server", Toast.ToastType.Error).Show();
                };
                if (contract != null)
                    World.INSTANCE.StartCoroutine(
                        MultiverseService.Instance.GetContract(network, contract, success, failure));
                else
                    World.INSTANCE.StartCoroutine(
                        MultiverseService.Instance.GetDefaultContract(success, failure));
            }
            else
                EnterMetaverse(null);
        }

        private static void OpenContractNotFoundDialog()
        {
            DialogService.INSTANCE.Show(new DialogConfig("Could not find the requested metaverse",
                    new Label("Do you want to enter the default metaverse?"))
                .WithCloseOnBackdropClick(false)
                .WithWidth(400)
                .WithHeight(100)
                .WithAction(new DialogAction("EXIT", () => GameManager.INSTANCE.Exit(), "utopia-button-warn"))
                .WithAction(new DialogAction("YES", () => Instance.ChangeContract(-1, null), "utopia-button-primary"))
            );
        }

        private void EnterMetaverse(Vector3? startingPosition)
        {
            if (!startingPosition.HasValue && WebBridge.IsPresent())
            {
                startingPosition = Player.GetSavedPosition();
                WebBridge.CallAsync<SerializableVector3>("getStartingPosition", "", (pos) =>
                {
                    GameManager.INSTANCE.SessionChanged(pos?.ToVector3() ?? startingPosition);
                    BrowserConnector.INSTANCE.ReportLoggedInUser(session);
                });
            }
            else
            {
                GameManager.INSTANCE.SessionChanged(startingPosition);
                BrowserConnector.INSTANCE.ReportLoggedInUser(session);
            }
        }

        public void Login()
        {
            if (!WebBridge.IsPresent())
                OpenCredentialsDialog();
            else
                ConnectProvider();
        }

        public void SetUpGuestSession()
        {
            SetupSessionFroWallet(Session.GUEST_WALLET);
        }

        private void SetupSessionFroWallet(string walletId)
        {
            var loaded = Session.Load();
            if (!loaded.HasNetwork() || string.IsNullOrWhiteSpace(loaded.Contract))
            {
                var loading = LoadingLayer.Show();
                World.INSTANCE.StartCoroutine(MultiverseService.Instance.GetDefaultContract(
                    s =>
                    {
                        SetSession(new Session(s.networkId, s.address, walletId));
                        loading.Close();
                    },
                    () =>
                    {
                        new Toast("Connection to server failed", Toast.ToastType.Error).Show();
                        loading.Close();
                    }));
            }
            else
                SetSession(new Session(loaded.Network, loaded.Contract, walletId));
        }

        private void OpenCredentialsDialog()
        {
            var loginCredentialsDialog = new LoginCredentialsDialog();
            DialogController dialog = null;
            dialog = DialogService.INSTANCE.Show(new DialogConfig("Login credentials", loginCredentialsDialog)
                .WithWidth(450)
                .WithHeight(300)
                .WithCancelAction()
                .WithAction(new DialogAction("Submit", () =>
                {
                    if (!loginCredentialsDialog.AreInputsValid())
                    {
                        new Toast("Invalid credentials", Toast.ToastType.Error).Show();
                        return;
                    }

                    SetupSessionFroWallet(loginCredentialsDialog.GetWallet());
                    dialog.Close();
                }, "utopia-button-secondary", false))
            );
        }
    }
}