using UnityEngine;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
    public ActionButton closeButton;

    void Start()
    {
        closeButton.AddListener(() =>
        {
            if (GameManager.INSTANCE.GetState() == GameManager.State.HELP)
                GameManager.INSTANCE.SetState(GameManager.State.PLAYING);
        });
    }
}
