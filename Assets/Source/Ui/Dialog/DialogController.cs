using UnityEngine.UIElements;

namespace Source.Ui.Dialog
{
    public class DialogController
    {
        private readonly int id;
        private readonly VisualElement dialog;

        public DialogController(int id, VisualElement dialog)
        {
            this.id = id;
            this.dialog = dialog;
        }

        public void Close()
        {
            DialogService.INSTANCE.Close(id);
        }

        public VisualElement Dialog => dialog;
    }
}