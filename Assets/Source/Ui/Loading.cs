using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Canvas
{
    public class Loading : MonoBehaviour
    {
        private static Loading instance;

        private Label label;
        private VisualElement image;
        private VisualElement root;
        private Sprite utopiaLogo;
        private Sprite errorLogo;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            image = root.Q<VisualElement>("image");
            label = root.Q<Label>("label");
        }

        private void Start()
        {
            instance = this;
            var manager = GameManager.INSTANCE;
            gameObject.SetActive(manager.GetState() == GameManager.State.LOADING);
            manager.stateChange.AddListener(state =>
                gameObject.SetActive(state == GameManager.State.LOADING)
            );
            utopiaLogo = Resources.Load<Sprite>("Icons/logo");
            errorLogo = Resources.Load<Sprite>("Icons/error");
        }

        public void UpdateText(string text)
        {
            label.text = text;
            UiImageUtils.SetBackground(image, utopiaLogo);
        }

        public void ShowConnectionError()
        {
            UpdateText("An Error Occured While Querying Blockchain\nTry Again Later");
            UiImageUtils.SetBackground(image, errorLogo);
        }

        public static Loading INSTANCE => instance;
    }
}