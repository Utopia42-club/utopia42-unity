using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owner : MonoBehaviour
{
    private ActionButton openProfileButton;

    public GameObject profileDialog;

    public ActionButton closeButton;

    // Start is called before the first frame update
    void Start()
    {
        openProfileButton = GetComponent<ActionButton>();
        GameManager.INSTANCE.stateChange.AddListener(state =>
        {
            profileDialog.SetActive(state == GameManager.State.PROFILE);
        });

        openProfileButton.AddListener(() =>
        {
            if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
                GameManager.INSTANCE.SetState(GameManager.State.PROFILE);
        });

        closeButton.AddListener(() =>
        {
            if (GameManager.INSTANCE.GetState() == GameManager.State.PROFILE)
                GameManager.INSTANCE.SetState(GameManager.State.PLAYING);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Profile"))
        {
            var state = GameManager.INSTANCE.GetState();
            if (state == GameManager.State.PROFILE)
                GameManager.INSTANCE.SetState(GameManager.State.PLAYING);
            else if (state == GameManager.State.PLAYING && IsOwned()) 
                GameManager.INSTANCE.SetState(GameManager.State.PROFILE);
        }
    }

    private bool IsOwned()
    {
        return false; // FIXME check if this land is owned
    }
}
