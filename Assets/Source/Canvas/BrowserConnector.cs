using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Source.Model;
using Source.Service.Ethereum;
using Source.Ui.Menu;
using Source.Utils;
using TMPro;
using UnityEngine;

namespace Source.Canvas
{
    public class BrowserConnector : MonoBehaviour
    {
        private string currentUrl;
        //[SerializeField]
        //private Button copyUrlButton;

        void Start()
        {
            //copyUrlButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = currentUrl);
        }

        public void EditProfile(Action onDone, Action onCancel)
        {
            if (WebBridge.IsPresent())
            {
#if UNITY_WEBGL
                var orig = WebGLInput.captureAllKeyboardInput;
                WebGLInput.captureAllKeyboardInput = false;
                WebBridge.Call<object>("editProfile", null);
                OpenDialog(
                    "Edit your profile on your browser. Click RELOAD when it is done"
                    ,() =>
                {
                    WebGLInput.captureAllKeyboardInput = orig;
                    onDone();
                }, () =>
                {
                    WebGLInput.captureAllKeyboardInput = orig;
                    onCancel();
                });
#endif
            }
            else
                CallUrl("editProfile", onDone, onCancel);
        }

        public void Transfer(long landId, Action onDone, Action onCancel)
        {
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("transfer", landId);
                OpenDialog(onDone, onCancel);
            }
            else
                CallUrl("transfer", landId.ToString(), onDone, onCancel);
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
                CallUrl("setNft", $"{landId}_{value}", onDone, onCancel);
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
                List<string> parameters = new List<string>();
                foreach (var l in lands)
                    parameters.Add(string.Join("_",
                        new long[] {l.startCoordinate.x, l.startCoordinate.z, l.endCoordinate.x, l.endCoordinate.z}));
                CallUrl("buy", string.Join(",", parameters), onDone, onCancel);
            }
        }

        public void ReportGameState(GameManager.State state)
        {
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("reportGameState", state.ToString());
            }
        }

        public void ReportPlayerState(AvatarController.PlayerState state)
        {
            if (WebBridge.IsPresent())
            {
                WebBridge.Call<object>("reportPlayerState", JsonConvert.SerializeObject(state));
            }
        }

        private void CallUrl(string method, string parameters, Action onDone, Action onCancel)
        {
            var wallet = Settings.WalletId();
            int network = EthereumClientService.INSTANCE.GetNetwork().id;
            if (parameters != null)
                currentUrl = string.Format("{0}?method={1}&param={2}&wallet={3}&network={4}",
                    Constants.WebAppHomeURL, method,
                    parameters, wallet, network);
            else
                currentUrl = string.Format("{0}?method={1}&wallet={2}&network={3}", Constants.WebAppHomeURL,
                    method, wallet,
                    network);

            Application.OpenURL(currentUrl);
            OpenDialog(onDone, onCancel);
        }

        private void CallUrl(string method, Action onDone, Action onCancel)
        {
            CallUrl(method, null, onDone, onCancel);
        }

        private void OpenDialog(Action onDone, Action onCancel)
        {
            OpenDialog(null, onDone, onCancel);
        }

        private void OpenDialog(String message, Action onDone, Action onCancel)
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog.WithContent("Dialog/TextContent");
            dialog.GetContent().GetComponent<TextMeshProUGUI>().text =
                message ?? "Accept the transaction on your browser. Click RELOAD when it is confirmed.";
            dialog.withOnClose(onCancel.Invoke);
            dialog.WithAction("CANCEL", () =>
            {
                manager.CloseDialog(dialog);
                onCancel.Invoke();
            });
            dialog.WithAction("RELOAD", () =>
            {
                manager.CloseDialog(dialog);
                onDone.Invoke();
            });
        }

        public static BrowserConnector INSTANCE
        {
            get { return GameObject.Find("BrowserConnector").GetComponent<BrowserConnector>(); }
        }
    }
}