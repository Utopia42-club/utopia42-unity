using Source.Model;
using Source.Service.Ethereum;
using Source.Ui.Dialog;
using Source.Ui.Snack;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Position = Source.Model.Position;

namespace Source.Ui.Login
{
    public class Login : MonoBehaviour
    {
        private static readonly string GUEST = "guest";

        private GameObject panel;
        private Button submitButton;
        private Button guestButton;

        private Vector3? startingPosition = null;
        private VisualElement root;
        private VisualElement walletLoginTile;
        private Button exitButton;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            walletLoginTile = root.Q<VisualElement>("walletLoginTile");
            guestButton = root.Q<Button>("guestButton");
            guestButton.clickable.clicked += SetGuest;
            submitButton = root.Q<Button>("enterButton");
            submitButton.clickable.clicked += Submit;
            exitButton = root.Q<Button>("exitButton");
            exitButton.tooltip = "Exit";
            exitButton.clickable.clicked += () => GameManager.INSTANCE.Exit();
            exitButton.AddManipulator(new ToolTipManipulator());
            var loadingId = LoadingLayer.LoadingLayer.Show(root);
            StartCoroutine(EthNetwork.GetNetworks(_ =>
                {
                    LoadingLayer.LoadingLayer.Hide(loadingId);
                    DoStart();
                },
                () =>
                {
                    SnackService.INSTANCE.Show(
                        new SnackConfig(
                            new Toast("Could not load any ETHEREUM networks. Please report the error.",
                                Toast.ToastType.Error)
                        )
                    );
                    LoadingLayer.LoadingLayer.Hide(loadingId);
                }));
        }

        private void DoStart()
        {
            var nets = EthNetwork.GetNetworksIfPresent();
            if (nets == null || nets.Length == 0)
            {
                SnackService.INSTANCE.Show(
                    new SnackConfig(
                        new Toast("Could not load any ETHEREUM networks. Please report the error.",
                            Toast.ToastType.Error)
                    )
                );
                return;
            }

            if (!WebBridge.IsPresent())
            {
                OpenCredentialsDialog();
            }
            else
            {
                var metamaskLoadingId = LoadingLayer.LoadingLayer.Show(walletLoginTile);
                WebBridge.CallAsync<ConnectionDetail>("connectMetamask", "", (ci) =>
                {
                    LoadingLayer.LoadingLayer.Hide(metamaskLoadingId);
                    if (ci.network.HasValue && ci.wallet != null)
                    {
                        PlayerPrefs.SetInt(Keys.NETWORK, ci.network.Value);
                        PlayerPrefs.SetString(Keys.WALLET, ci.wallet);
                    }
                    else
                        OpenCredentialsDialog();
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
            var dialogId = 0;
            dialogId = DialogService.INSTANCE.Show(new DialogConfig("Login credentials", loginCredentialsDialog)
                .WithWidth(450)
                .WithHeight(300)
                .WithCancelAction()
                .WithAction(new DialogAction("Submit", () =>
                {
                    loginCredentialsDialog.SaveInputs();
                    Submit();
                }, "utopia-stroked-button-secondary"))
            );
        }

        public void SetGuest()
        {
            DoSubmit(GUEST);
        }

        public void Submit()
        {
            if (string.IsNullOrWhiteSpace(WalletId()))
                OpenCredentialsDialog();
            else
                DoSubmit(WalletId());
        }


        private void DoSubmit(string walletId)
        {
            GameManager.INSTANCE.SettingsChanged(Network(), startingPosition);
        }

        public static bool IsGuest()
        {
            return WalletId().Equals(GUEST);
        }

        public static string WalletId()
        {
            return PlayerPrefs.GetString(Keys.WALLET).ToLower();
        }

        public static EthNetwork Network()
        {
            var nets = EthNetwork.GetNetworksIfPresent();
            return EthNetwork.GetByIdIfPresent(PlayerPrefs.GetInt(Keys.NETWORK,
                nets == null || nets.Length == 0 ? -1 : nets[0].id));
        }

        public static ConnectionDetail ConnectionDetail()
        {
            var detail = new ConnectionDetail();
            detail.wallet = WalletId();
            var net = Network();
            detail.network = net?.id ?? -1;
            return detail;
        }
    }

    class Keys
    {
        public static readonly string WALLET = "WALLET";
        public static readonly string NETWORK = "NETWORK";
    }
}