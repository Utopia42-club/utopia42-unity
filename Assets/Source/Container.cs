using UnityEngine;

namespace Source
{
    public class Container : MonoBehaviour
    {
        public Menu menu;
        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = GameManager.INSTANCE;
        }
    }
}