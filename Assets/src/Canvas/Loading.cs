using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField]
    private Text textComponent;

    private void Start()
    {
        var manager = GameManager.INSTANCE;
        this.gameObject.SetActive(manager.GetSTate() == GameManager.State.LOADING);
        manager.stateChange.AddListener(state =>
            this.gameObject.SetActive(state == GameManager.State.LOADING)
        );
    }

    public void UpdateText(string text)
    {
        this.textComponent.text = text;
    }

    public static Loading INSTANCE
    {
        get
        {
            return GameObject.Find("Loading").GetComponent<Loading>();
        }
    }
}
