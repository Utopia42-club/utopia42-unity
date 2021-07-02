using UnityEngine;

public class Map : MonoBehaviour
{
    void Start()
    {
        GameManager.INSTANCE.stateChange.AddListener(
            state => gameObject.SetActive(state == GameManager.State.MAP)
        );
    }
}
