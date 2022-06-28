using System.Linq;
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
            guestButton.clickable.clicked += () =>
            {
                AuthService.SetGuestMode();
                DoSubmit();
            };

            submitButton = root.Q<Button>("enterButton");
            submitButton.clickable.clicked += Submit;

            exitButton = root.Q<Button>("exitButton");
            exitButton.tooltip = "Exit";
            exitButton.clickable.clicked += () => GameManager.INSTANCE.Exit();
            exitButton.AddManipulator(new ToolTipManipulator());
            
            var loading = LoadingLayer.LoadingLayer.Show(root);
            GameManager.INSTANCE.StartCoroutine(EthNetwork.GetNetworks(_ =>
                {
                    loading.Close();
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

        private void Submit()
        {
            if (!WebBridge.IsPresent())
                OpenCredentialsDialog();
            else
            {
                var metamaskLoading = LoadingLayer.LoadingLayer.Show(walletLoginTile);
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
            DialogService.INSTANCE.Show(new DialogConfig("Login credentials", loginCredentialsDialog)
                .WithWidth(450)
                .WithHeight(300)
                .WithCancelAction()
                .WithAction(new DialogAction("Submit", () =>
                {
                    loginCredentialsDialog.SaveInputs();
                    if (!string.IsNullOrEmpty(AuthService.WalletId()))
                        DoSubmit();
                    else
                        SnackService.INSTANCE.Show(new SnackConfig(
                            new Toast("Invalid Wallet address", Toast.ToastType.Error)
                        ));
                }, "utopia-stroked-button-secondary"))
            );
        }

        private void DoSubmit()
        {
            GameManager.INSTANCE.SettingsChanged(AuthService.Network(), startingPosition);
        }
    }
}