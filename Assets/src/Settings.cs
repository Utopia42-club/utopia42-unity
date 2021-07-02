using UnityEngine;
using UnityEngine.UI;

class Keys
{
    public static readonly string WALLET = "WALLET";
    public static readonly string NETWORK = "NETWORK";
}

public class Settings : MonoBehaviour
{
    private static readonly string GUEST = "guest";
    public InputField walletInput;
    public Dropdown networkInput;
    public Button submitButton;
    public Button saveGameButton;

    void Start()
    {
        foreach (var net in EthNetwork.NETWORKS)
            networkInput.options.Add(new Dropdown.OptionData(net.name));

        ResetInputs();

        saveGameButton.onClick.AddListener(() => GameManager.INSTANCE.Save());
        walletInput.onEndEdit.AddListener((text) => ResetButtonsState());

        GameManager.INSTANCE.stateChange.AddListener(state =>
        {
            ResetInputs();
            gameObject.SetActive(state == GameManager.State.SETTINGS);
        });
    }

    private void ResetInputs()
    {
        walletInput.text = IsGuest() ? null : WalletId();
        var net = Network();
        networkInput.value = -1;

        for (int i = 0; i < networkInput.options.Count; i++)
        {
            if (EthNetwork.NETWORKS[i].Equals(net))
            {
                networkInput.value = i;
                break;
            }
        }
        networkInput.interactable = !EthereumClientService.INSTANCE.IsInited();
        saveGameButton.gameObject.SetActive(EthereumClientService.INSTANCE.IsInited());
        saveGameButton.interactable = !IsGuest();
    }

    private void ResetButtonsState()
    {
        submitButton.interactable = !string.IsNullOrEmpty(walletInput.text);
    }

    void Update()
    {
        ResetButtonsState();
    }

    public void SetGuest()
    {
        DoSubmit(GUEST);
    }

    public void Submit()
    {
        if (string.IsNullOrWhiteSpace(walletInput.text)) return;
        DoSubmit(walletInput.text);
    }

    private void DoSubmit(string walletId)
    {
        if (networkInput.interactable)
        {
            PlayerPrefs.SetInt(Keys.NETWORK, EthNetwork.NETWORKS[networkInput.value].id);
            PlayerPrefs.SetString(Keys.WALLET, walletId);
            GameManager.INSTANCE.SettingsChanged();
            return;
        }

        if (Equals(WalletId(), walletId))
        {
            GameManager.INSTANCE.ExitSettings();
            return;
        }

        PlayerPrefs.SetString(Keys.WALLET, walletId);
        GameManager.INSTANCE.SettingsChanged();
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
        return EthNetwork.GetById(PlayerPrefs.GetInt(Keys.NETWORK, EthNetwork.NETWORKS[0].id));
    }
}
