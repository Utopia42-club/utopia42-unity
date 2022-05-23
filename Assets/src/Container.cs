using src;
using UnityEngine;

public class Container : MonoBehaviour
{
    public TabMenu tabMenu;
    private GameManager _gameManager;

    void Start()
    {
        tabMenu.gameObject.SetActive(false);
        _gameManager = GameManager.INSTANCE;
        _gameManager.stateChange.AddListener(state =>
        {
            if (_gameManager.IsWorldInited() && (state == GameManager.State.SETTINGS || state == GameManager.State.MAP))
                tabMenu.gameObject.SetActive(true);
            else
                tabMenu.gameObject.SetActive(false);

            if (tabMenu.gameObject.activeSelf)
                switch (state)
                {
                    case GameManager.State.MAP:
                        tabMenu.SelectTabByIndex(0);
                        break;
                    case GameManager.State.SETTINGS:
                        tabMenu.SelectTabByIndex(1);
                        break;
                }
        });
    }
}