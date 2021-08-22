using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

public class Dialog : MonoBehaviour
{
    private TextMeshProUGUI headerLabel;
    private ActionButton closeButton;
    private GameObject contentWrapper;
    private GameObject headerWrapper;
    private GameObject container;
    private List<UnityAction> onCloseActions = new List<UnityAction>();

    public GameObject content;

    public string headerText;

    void Start()
    {
        container = GameObject.FindGameObjectWithTag("Container");
        contentWrapper = GameObject.FindGameObjectWithTag("Content");

        content.transform.SetParent(contentWrapper.transform);

        headerWrapper = GameObject.FindGameObjectWithTag("Header");
        headerLabel = headerWrapper.GetComponentInChildren<TextMeshProUGUI>();
        closeButton = headerWrapper.GetComponentInChildren<ActionButton>();

        headerLabel.SetText(headerText);
        closeButton.AddListener(() =>
        {
            Close();
            foreach (var action in onCloseActions)
                action.Invoke();
        });
        Close();
    }

    internal void Close()
    {
        container.SetActive(false);
    }

    public void Open()
    {
        container.SetActive(true);
    }


    public void AddOnCloseAction(UnityAction action)
    {
        onCloseActions.Add(action);
    }


}
