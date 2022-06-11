using UnityEngine;

namespace src.Environment
{
    public class SpotLight : MonoBehaviour
    {
        public Light spotLight;

        void Start()
        {
            spotLight.range = 20;
            GameManager.INSTANCE.stateChange.AddListener(state =>
                {
                    gameObject.SetActive(state == GameManager.State.PLAYING || state == GameManager.State.MOVING_OBJECT);
                }
            );
        }

        void Update()
        {
            if (Input.GetButtonDown("Light"))
            {
                spotLight.range = spotLight.range > 0 ? 0 : 20;
            }
        }
    }
}