using UnityEngine;

public class StateAware : MonoBehaviour
{
    [SerializeField]
    private GameManager.State state;

    void Start()
    {
        GameManager.INSTANCE.stateChange.AddListener(s =>
        {
            gameObject.SetActive(s == state);
        });
    }
}
