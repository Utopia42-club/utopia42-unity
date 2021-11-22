using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private static readonly string GUEST = "guest";
    public Dropdown networkInput;
    public Button submitButton;
    public Button saveGameButton;
    public Button editProfileButton;
    public Button helpButton;
    public Slider slider;
    public Text text;

    void Start()
    {
        foreach (var net in EthNetwork.NETWORKS)
            networkInput.options.Add(new Dropdown.OptionData(net.name));

        ResetInputs();
        var manager = GameManager.INSTANCE;
        saveGameButton.onClick.AddListener(() => manager.Save());
        editProfileButton.onClick.AddListener(() => manager.ShowUserProfile());
        helpButton.onClick.AddListener(() => manager.Help());

        manager.stateChange.AddListener(state =>
        {
            ResetInputs();
            gameObject.SetActive(state == GameManager.State.SETTINGS);
        });
        if (WebBridge.IsPresent())
        {
            WebBridge.CallAsync<ConnectionDetail>("connectMetamask", "", (ci) =>
            {
                if (ci.network.HasValue && ci.wallet != null)
                {
                    PlayerPrefs.SetInt(Keys.NETWORK, ci.network.Value);
                    PlayerPrefs.SetString(Keys.WALLET, ci.wallet);
                    ResetInputs();
                }
            });
        }
    }

    private void ResetInputs()
    {
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
        saveGameButton.interactable = !IsGuest() && VoxelService.INSTANCE.HasChange();
        editProfileButton.interactable = !IsGuest();

        saveGameButton.gameObject.SetActive(EthereumClientService.INSTANCE.IsInited());
        editProfileButton.gameObject.SetActive(EthereumClientService.INSTANCE.IsInited());
        helpButton.gameObject.SetActive(EthereumClientService.INSTANCE.IsInited());
    }

    private void ResetButtonsState()
    {
    }

    void Update()
    {
        text.text = string.Format("Textures: {0}, Block Types: {1}", slider.value, slider.value*29);
        ResetButtonsState();
    }

    public void SetGuest()
    {
        Chunk.MAT_COUNT = (int)slider.value;
        DoSubmit(GUEST);
    }

    public void Submit()
    {
    }

    private void DoSubmit(string walletId)
    {
        if (networkInput.interactable)
            PlayerPrefs.SetInt(Keys.NETWORK, EthNetwork.NETWORKS[networkInput.value].id);
        else if (Equals(WalletId(), walletId))
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

    public static ConnectionDetail ConnectionDetail()
    {
        var detail = new ConnectionDetail();
        detail.wallet = WalletId();
        detail.network = Network().id;
        return detail;
    }
}

class Keys
{
    public static readonly string WALLET = "WALLET";
    public static readonly string NETWORK = "NETWORK";
}
