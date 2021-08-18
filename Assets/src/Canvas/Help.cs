using UnityEngine;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
    [SerializeField]
    private Button okButton;

    void Start()
    {
        okButton.onClick.AddListener(() => GameManager.INSTANCE.SetState(GameManager.State.PLAYING));
    }
}
