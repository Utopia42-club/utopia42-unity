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
            var assetsInventory = AssetsInventory.AssetsInventory.INSTANCE;
            if (assetsInventory != null && assetsInventory.IsOpen())
                return;
            if (Input.GetButtonDown("Light"))
            {
                spotLight.range = spotLight.range > 0 ? 0 : 20;
            }
        }
    }
}