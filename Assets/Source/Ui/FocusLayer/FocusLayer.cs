using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.FocusLayer
{
    public class FocusLayer : MonoBehaviour
    {
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
                    MouseLook.INSTANCE.LockCursor();
            });
        }
    }
}