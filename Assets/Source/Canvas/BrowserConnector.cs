using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Source.Configuration;
using Source.Model;
using Source.Service.Auth;
using Source.Ui.Dialog;
using Source.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Canvas
{
    public class BrowserConnector : MonoBehaviour
    {
        private string currentUrl;

        public static BrowserConnector INSTANCE => GameObject.Find("BrowserConnector").GetComponent<BrowserConnector>();

        public void OpenDApp(Action onDone, Action onCancel)
        {
            const string msg = "Edit your profile on the app. Click RELOAD when it is saved.";
            Application.OpenURL(Configurations.Instance.dAppUrl);
            OpenDialog(onDone, onCancel, msg);
        }

        public void Transfer(long landId, Action onDone, Action onCancel)
        {
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("transfer", landId);
                OpenDialog(onDone, onCancel);
            }
            else
            {
                CallUrl("transfer", landId.ToString(), onDone, onCancel);
            }
        }

        public void SetNft(long landId, bool value, Action onDone, Action onCancel)
        {
            if (WebBridge.IsPresent())
            {
                var data = new Dictionary<string, object>();
                data.Add("landId", landId);
                data.Add("nft", value);
                WebBridge.Call<object>("setNft", data);
                OpenDialog(onDone, onCancel);
            }
            else
            {
                CallUrl("setNft", $"{landId}_{value}", onDone, onCancel);
            }
        }

        public void Save(Dictionary<long, string> data, Action onDone, Action onCancel)
        {
            if (data.Count == 0) onDone();
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("save", data);
                OpenDialog(onDone, onCancel);
            }
            else
            {
                var values = new List<string>();
                foreach (var d in data)
                    values.Add(string.Join("_", d.Key, d.Value));
                CallUrl("save", string.Join(",", values), onDone, onCancel);
            }
        }

        public void Buy(List<Land> lands, Action onDone, Action onCancel)
        {
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("buy", lands);
                OpenDialog(onDone, onCancel);
            }
            else
            {
                var parameters = new List<string>();
                foreach (var l in lands)
                    parameters.Add(string.Join("_",
                        new long[] {l.startCoordinate.x, l.startCoordinate.z, l.endCoordinate.x, l.endCoordinate.z}));
                CallUrl("buy", string.Join(",", parameters), onDone, onCancel);
            }
        }

        public void ReportGameState(GameManager.State state)
        {
            if (WebBridge.IsPresent()) WebBridge.Call<object>("reportGameState", state.ToString());
        }

        public void ReportSession(Session session)
        {
            if (WebBridge.IsPresent()) WebBridge.Call<object>("reportSession", session);
        }

        public void ReportPlayerState(AvatarController.PlayerState state)
        {
            if (WebBridge.IsPresent()) WebBridge.Call<object>("reportPlayerState", JsonConvert.SerializeObject(state));
        }

        private void CallUrl(string method, string parameters, Action onDone, Action onCancel, string message = null)
        {
            var wallet = AuthService.Instance.WalletId();
            var contract = AuthService.Instance.CurrentContract;
            currentUrl =
                $"{Configurations.Instance.webAppRpcURL}?method={method}&wallet={wallet}&network={contract.network.id}&contract={contract.id}";
            if (parameters != null)
                currentUrl = $"{currentUrl}&param={parameters}";
            Application.OpenURL(currentUrl);
            OpenDialog(onDone, onCancel, message);
        }

        private void CallUrl(string method, Action onDone, Action onCancel, string message = null)
        {
            CallUrl(method, null, onDone, onCancel, message);
        }

        private void OpenDialog(Action onDone, Action onCancel, string message = null)
        {
            OpenDialog(message, onDone, onCancel);
        }

        private void OpenDialog(string message, Action onDone, Action onCancel)
        {
            var label = new Label
            {
                text = message ?? "Accept the transaction on your browser. Click RELOAD when it is confirmed."
            };
            DialogService.INSTANCE.Show(
                new DialogConfig(label)
                    .WithCloseOnBackdropClick(false)
                    .WithCancelAction(onCancel)
                    .WithAction(new DialogAction("Reload", onDone.Invoke, "utopia-button-secondary"))
                    .WithOnClose(onCancel)
            );
        }
    }
}