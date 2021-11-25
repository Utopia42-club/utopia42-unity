using UnityEngine;

namespace src.Canvas
{
    public class Help : MonoBehaviour
    {
        public ActionButton closeButton;

        void Start()
        {
            closeButton.AddListener(() =>
            {
                if (GameManager.INSTANCE.GetState() == GameManager.State.HELP)
                    GameManager.INSTANCE.ReturnToGame();
            });
        }
    }
}
