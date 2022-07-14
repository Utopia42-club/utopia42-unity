using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public class UiStateAware : MonoBehaviour
    {
        [SerializeField] private GameManager.State[] states;

        void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(UpdateVisibility);
            UpdateVisibility(GameManager.INSTANCE.GetState());
        }

        private void UpdateVisibility(GameManager.State s)
        {
            var active = states.Any(state => s == state);
            GetComponent<UIDocument>().rootVisualElement?.SetEnabled(active);
            gameObject.SetActive(active);
        }
    }
}