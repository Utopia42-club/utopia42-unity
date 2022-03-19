using TMPro;
using UnityEngine;

namespace src.Canvas
{
    public class HelpMessage : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        public Shortcut shortcut;

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
                    shortcut.gameObject.SetActive(true);
                    shortcut.SetShortcut("F2");
                    textMesh.SetText("Settings");
                    break;
                case (GameManager.State.MAP):
                    gameObject.SetActive(true);
                    shortcut.gameObject.SetActive(true);
                    shortcut.SetShortcut("F2");
                    textMesh.SetText("Toggle Side Panel");
                    break;
                default:
                    gameObject.SetActive(false);
                    shortcut.gameObject.SetActive(false);
                    break;
            }
        }
    }
}