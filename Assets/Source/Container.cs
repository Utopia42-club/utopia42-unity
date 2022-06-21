using Source;
using UnityEngine;

public class Container : MonoBehaviour
{
    public Menu menu;
    private GameManager _gameManager;

    void Start()
    {
        _gameManager = GameManager.INSTANCE;
    }
}