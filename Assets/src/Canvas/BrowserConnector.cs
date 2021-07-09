using System;
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

    public void Call(string method, string parameters, Action onDone, Action onCancel)
    {
        var wallet = Settings.WalletId();
        int network = EthereumClientService.INSTANCE.GetNetwork().id;
        currentUrl = string.Format("{0}/{1}/{2}?wallet={3}&networkId={4}", WEB_APP_URL, method, parameters, wallet, network);
        Application.OpenURL(currentUrl);
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
