using UnityEngine;
using UnityEngine.UI;

class Keys
{
    public static readonly string WALLET = "WALLET";
}

public class Settings : MonoBehaviour
{
    private static readonly string GUEST = "GUEST";
    public InputField walletInput;
    public Button submitButton;

    void Start()
    {
        ResetInputText();

        walletInput.onEndEdit.AddListener((text) => ResetButtonState());

        GameManager.INSTANCE.stateChange.AddListener(state =>
        {
            ResetInputText();
            gameObject.SetActive(state == GameManager.State.SETTINGS);
        });
    }

    private void ResetInputText()
    {
        walletInput.text = IsGuest() ? null : WalletId();
    }

    private void ResetButtonState()
    {
        submitButton.interactable = !string.IsNullOrEmpty(walletInput.text);
    }

    void Update()
    {
        ResetButtonState();
    }

    public void SetGuest()
    {
        SetWallet(GUEST);
    }

    public void SetWalletId()
    {
        if (string.IsNullOrWhiteSpace(walletInput.text)) return;
        SetWallet(walletInput.text);
    }

    private void SetWallet(string id)
    {
        if(Equals(WalletId(), id))
        {
            GameManager.INSTANCE.ExitSettings();
            return;
        }
        PlayerPrefs.SetString(Keys.WALLET, id);
        GameManager.INSTANCE.WalletChanged();
    }

    public static bool IsGuest()
    {
        return WalletId().Equals(GUEST);
    }

    public static string WalletId()
    {
        return PlayerPrefs.GetString(Keys.WALLET);
    }

}
