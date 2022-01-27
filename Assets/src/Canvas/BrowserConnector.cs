using System;
using System.Collections.Generic;
using src.Model;
using src.Service.Ethereum;
using src.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class BrowserConnector : MonoBehaviour
    {
        private string currentUrl;
        [SerializeField] private Button doneButton;

        [SerializeField] private Button cancelButton;
        //[SerializeField]
        //private Button copyUrlButton;

        void Start()
        {
            var manager = GameManager.INSTANCE;
            gameObject.SetActive(manager.GetState() == GameManager.State.BROWSER_CONNECTION);
            manager.stateChange.AddListener(state =>
            {
                gameObject.SetActive(state == GameManager.State.BROWSER_CONNECTION);
            });
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
                ResetButtons(() =>
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
                ResetButtons(onDone, onCancel);
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
                ResetButtons(onDone, onCancel);
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
                ResetButtons(onDone, onCancel);
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
                ResetButtons(onDone, onCancel);
            }
            else
            {
                List<string> parameters = new List<string>();
                foreach (var l in lands)
                    parameters.Add(string.Join("_", new long[] {l.x1, l.y1, l.x2, l.y2}));
                CallUrl("buy", string.Join(",", parameters), onDone, onCancel);
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
            ResetButtons(onDone, onCancel);
        }

        private void CallUrl(string method, Action onDone, Action onCancel)
        {
            CallUrl(method, null, onDone, onCancel);
        }

        private void ResetButtons(Action onDone, Action onCancel)
        {
            cancelButton.onClick.RemoveAllListeners();
            doneButton.onClick.RemoveAllListeners();
            doneButton.onClick.AddListener(() => onDone.Invoke());
            cancelButton.onClick.AddListener(() => onCancel.Invoke());
        }

        public static BrowserConnector INSTANCE
        {
            get { return GameObject.Find("BrowserConnector").GetComponent<BrowserConnector>(); }
        }
    }
}