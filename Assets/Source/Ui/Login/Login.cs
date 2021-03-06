using Source.Canvas;
using Source.Service.Ethereum;
using Source.Ui.Dialog;
using Source.Ui.Snack;
using UnityEngine;
using UnityEngine.UIElements;
using Position = Source.Model.Position;

namespace Source.Ui.Login
{
    public class Login : MonoBehaviour
    {
        private GameObject panel;
        private Button submitButton;
        private Button guestButton;

        private Vector3? startingPosition = null;
        private VisualElement root;
        private VisualElement memberTile;
        private VisualElement guestTile;
        private Button guestTabButton;
        private Button memberTabButton;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            memberTile = root.Q<VisualElement>("memberTile");
            guestTile = root.Q<VisualElement>("guestTile");

            guestButton = root.Q<Button>("guestButton");
            guestButton.clickable.clicked += () =>
            {
                AuthService.SetGuestMode();
                DoSubmit();
            };

            submitButton = root.Q<Button>("enterButton");
            submitButton.clickable.clicked += Submit;

            memberTabButton = root.Q<Button>("memberTabButton");
            memberTabButton.clickable.clicked += () => SelectTab(0);
            guestTabButton = root.Q<Button>("guestTabButton");
            guestTabButton.clickable.clicked += () => SelectTab(1);

            var loading = LoadingLayer.LoadingLayer.Show(root);
            GameManager.INSTANCE.StartCoroutine(EthNetwork.GetNetworks(_ => { loading.Close(); },
                () =>
                {
                    new Toast("Could not load any ETHEREUM networks. Please report the error.",
                        Toast.ToastType.Error).Show();
                    loading.Close();
                }));
            SelectTab(0);
        }

        private void SelectTab(int index)
        {
            switch (index)
            {
                case 0:
                    guestTile.style.display = DisplayStyle.None;
                    memberTile.style.display = DisplayStyle.Flex;
                    guestTabButton.RemoveFromClassList("selected-login-tab-button");
                    memberTabButton.AddToClassList("selected-login-tab-button");
                    break;
                case 1:
                    guestTile.style.display = DisplayStyle.Flex;
                    memberTile.style.display = DisplayStyle.None;
                    guestTabButton.AddToClassList("selected-login-tab-button");
                    memberTabButton.RemoveFromClassList("selected-login-tab-button");
                    break;
            }
        }

        private void Submit()
        {
            if (!WebBridge.IsPresent())
                OpenCredentialsDialog();
            else
            {
                var metamaskLoading = LoadingLayer.LoadingLayer.Show(memberTile);
                AuthService.Connect(detail =>
                {
                    metamaskLoading.Close();
                    if (detail.network == null || detail.wallet == null)
                        OpenCredentialsDialog();
                    else
                        DoSubmit();
                });
                WebBridge.CallAsync<Position>("getStartingPosition", "", (pos) =>
                {
                    if (pos == null)
                        startingPosition = null;
                    else
                        startingPosition = new Vector3(pos.x, pos.y, pos.z);
                });
            }
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

                    loginCredentialsDialog.SaveInputs();
                    DoSubmit();
                    dialog.Close();
                }, "utopia-button-secondary", false))
            );
        }

        private void DoSubmit()
        {
            GameManager.INSTANCE.SettingsChanged(AuthService.Network(), startingPosition);
            BrowserConnector.INSTANCE.ReportLoggedInUser(new User(
                AuthService.WalletId(), AuthService.IsGuest()
            ));
        }
    }

    public class User
    {
        private string walletId;
        private bool isGuest;

        public User(string walletId, bool isGuest)
        {
            this.walletId = walletId;
            this.isGuest = isGuest;
        }

        public string WalletId => walletId;

        public bool IsGuest => isGuest;
    }
}