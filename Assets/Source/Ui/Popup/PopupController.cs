using UnityEngine.UIElements;

namespace Source.Ui.Popup
{
    public class PopupController
    {
        private readonly int id;
        private readonly VisualElement popup;

        public PopupController(int id, VisualElement popup)
        {
            this.id = id;
            this.popup = popup;
        }

        public void Close()
        {
            PopupService.INSTANCE.Close(id);
        }

        public VisualElement Popup => popup;
    }
}