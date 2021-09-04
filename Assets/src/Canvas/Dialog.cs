using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    [SerializeField]
    private GameObject actionPrefab;
    [SerializeField]
    private TextMeshProUGUI headerLabel;
    [SerializeField]
    private ActionButton closeButton;
    [SerializeField]
    private GameObject contentWrapper;
    [SerializeField]
    private GameObject footer;
    private List<UnityAction> onCloseActions = new List<UnityAction>();
    private GameObject content;

    void Start()
    {
        closeButton.AddListener(Close);
    }

    public Dialog WithTitle(string title)
    {
        headerLabel.text = title;
        return this;
    }

    public Dialog WithContent(string prefab)
    {
        content = Instantiate(Resources.Load<GameObject>(prefab), contentWrapper.transform);
        return this;
    }

    public GameObject GetContent()
    {
        return content;
    }

    public Dialog WithAction(string text, UnityAction action)
    {
        GameObject go = Instantiate(actionPrefab, footer.transform);
        go.GetComponent<Button>().onClick.AddListener(action);
        go.GetComponentInChildren<TextMeshProUGUI>().text = text;
        return this;
    }

    private void Close()
    {
        GameManager.INSTANCE.CloseDialog(this);
    }
}