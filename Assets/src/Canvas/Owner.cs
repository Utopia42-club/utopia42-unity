using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owner : MonoBehaviour
{
    private ActionButton openProfileButton;

    public GameObject profileDialog;

    // Start is called before the first frame update
    void Start()
    {
        openProfileButton = GetComponent<ActionButton>();
        GameManager.INSTANCE.stateChange.AddListener(state =>
        {
            profileDialog.SetActive(state == GameManager.State.PROFILE);
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

        if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING && openProfileButton.isPressed())
            GameManager.INSTANCE.SetState(GameManager.State.PROFILE);
    }

    private bool IsOwned()
    {
        return false; // FIXME check if this land is owned
    }
}
