using UnityEngine;

namespace Source.Environment
{
    public class SpotLight : MonoBehaviour
    {
        public Light spotLight;

        private void Start()
        {
            spotLight.range = 20;
            GameManager.INSTANCE.stateChange.AddListener(state =>
                {
                    gameObject.SetActive(state == GameManager.State.PLAYING);
                }
            );
        }

        private void Update()
        {
            if (!GameManager.INSTANCE.IsTextInputFocused() && Input.GetButtonDown("Light"))
                spotLight.range = spotLight.range > 0 ? 0 : 20;
        }
    }
}