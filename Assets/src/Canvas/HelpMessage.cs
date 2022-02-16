using TMPro;
using UnityEngine;

namespace src.Canvas
{
    public class HelpMessage : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;

        void Start()
        {
            var gameManager = GameManager.INSTANCE;
            gameManager.stateChange.AddListener(UpdateView);
            UpdateView(gameManager.GetState());
        }

        private void UpdateView(GameManager.State s)
        {
            switch (s)
            {
                case (GameManager.State.PLAYING):
                    gameObject.SetActive(true);
                    textMesh.SetText("F2: Open Settings");
                    break;
                case (GameManager.State.MAP):
                    gameObject.SetActive(true);
                    textMesh.SetText("F2: Open Side Panel");
                    break;
                default:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }
}