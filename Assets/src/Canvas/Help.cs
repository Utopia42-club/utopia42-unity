using UnityEngine;

namespace src.Canvas
{
    public class Help : MonoBehaviour
    {
        public ActionButton closeButton;

        void Start()
        {
            var gameManager = GameManager.INSTANCE;
            closeButton.AddListener(() => { gameManager.ReturnToGame(); });
            gameManager.stateGuards.Add(
                (currentState, nextState) =>
                    !(gameObject.activeSelf && currentState == GameManager.State.HELP));
        }
    }
}