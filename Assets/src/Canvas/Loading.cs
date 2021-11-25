using TMPro;
using UnityEngine;

namespace src.Canvas
{
    public class Loading : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textComponent;

        private void Start()
        {
            var manager = GameManager.INSTANCE;
            this.gameObject.SetActive(manager.GetState() == GameManager.State.LOADING);
            manager.stateChange.AddListener(state =>
                this.gameObject.SetActive(state == GameManager.State.LOADING)
            );
        }

        public void UpdateText(string text)
        {
            this.textComponent.text = text;
        }

        public static Loading INSTANCE
        {
            get { return GameObject.Find("Loading").GetComponent<Loading>(); }
        }
    }
}