using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BrowserConnector : MonoBehaviour
{
    private static string WEB_APP_URL = "http://app.utopia42.club";
    private string currentUrl;
    [SerializeField]
    private Button doneButton;
    [SerializeField]
    private Button cancelButton;
    //[SerializeField]
    //private Button copyUrlButton;

    void Start()
    {
        var manager = GameManager.INSTANCE;
        gameObject.SetActive(manager.GetSTate() == GameManager.State.BROWSER_CONNECTION);
        manager.stateChange.AddListener(state =>
            gameObject.SetActive(state == GameManager.State.BROWSER_CONNECTION)
        );
        //copyUrlButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = currentUrl);
    }

    public void Save(List<string> lands, Action onDone, Action onCancel)
    {
        if (WebBridge.IsPresent())
        {
            WebBridge.Call<object>("save", lands);
            ResetButtons(onDone, onCancel);
        }
        else
            CallUrl("save", string.Join(",", lands), onDone, onCancel);
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
            List<long> parameters = new List<long>();
            foreach (var l in lands)
            {
                parameters.Add(l.x1);
                parameters.Add(l.y1);
                parameters.Add(l.x2);
                parameters.Add(l.y2);
            }
            CallUrl("buy", string.Join(",", parameters), onDone, onCancel);
        }
    }

    private void CallUrl(string method, string parameters, Action onDone, Action onCancel)
    {
        var wallet = Settings.WalletId();
        int network = EthereumClientService.INSTANCE.GetNetwork().id;
        currentUrl = string.Format("{0}/{1}/{2}?wallet={3}&networkId={4}", WEB_APP_URL, method, parameters, wallet, network);
        Application.OpenURL(currentUrl);
        ResetButtons(onDone, onCancel);
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
        get
        {
            return GameObject.Find("BrowserConnector").GetComponent<BrowserConnector>();
        }
    }
}
