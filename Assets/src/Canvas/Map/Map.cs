using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private Button saveButton;
        [SerializeField] private RectPane pane;
        [SerializeField] private LandProfileDialog landProfileDialog;

        void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(
                state => gameObject.SetActive(state == GameManager.State.MAP)
            );
            saveButton.onClick.AddListener(DoSave);
        }

        private void Update()
        {
            saveButton.gameObject.SetActive(pane.HasDrawn());
        }

        private void DoSave()
        {
            GameManager.INSTANCE.Buy(pane.GetDrawn());
        }

        public void ChangeLandProfileDialogState(bool state)
        {
            landProfileDialog.gameObject.SetActive(state);
        }
    }
}