using TMPro;
using UnityEngine;

namespace src.Canvas
{
    public class Loading : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private GameObject logo;
        [SerializeField] private GameObject errorImage;

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

        public void ShowConnectionError()
        {
            logo.gameObject.SetActive(false);
            errorImage.gameObject.SetActive(true);
            UpdateText("An Error Occured While Querying Blockchain\nTry Again Later");
        }

        public static Loading INSTANCE
        {
            get { return GameObject.Find("Loading").GetComponent<Loading>(); }
        }
    }
}