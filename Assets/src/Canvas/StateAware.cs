using UnityEngine;

namespace src.Canvas
{
    public class StateAware : MonoBehaviour
    {
        [SerializeField]
        private GameManager.State[] states;

        void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(s =>
            {
                foreach (var state in states)
                {
                    if (s == state)
                    {
                        gameObject.SetActive(true);
                        return;
                    }
                }
                gameObject.SetActive(false);
            });
        }
    }
}
