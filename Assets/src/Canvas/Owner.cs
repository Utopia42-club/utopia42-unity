using UnityEngine;
using TMPro;

public class Owner : MonoBehaviour
{
    public TextMeshProUGUI label;

    public ActionButton openProfileButton;

    public GameObject profileDialog;

    public ActionButton closeButton;

    private Profile owner;

    // Start is called before the first frame update
    void Start()
    {
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
            else if (state == GameManager.State.PLAYING && owner != null)
                GameManager.INSTANCE.SetState(GameManager.State.PROFILE);
        }
    }

    public void setOwner(Profile profile)
    {
        owner = profile;
        label.SetText(profile.name);
    }

    public Profile getOwner()
    {
        return owner;
    }
}
