using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private RectPane pane;

    void Start()
    {
        GameManager.INSTANCE.stateChange.AddListener(
            state => gameObject.SetActive(state == GameManager.State.MAP)
        );
        saveButton.onClick.AddListener(DoSave);
    }

    private void Update()
    {
        saveButton.gameObject.SetActive(pane.HasDrawn());
    }

    private void DoSave()
    {
        GameManager.INSTANCE.Buy(pane.GetDrawn());
    }
}
