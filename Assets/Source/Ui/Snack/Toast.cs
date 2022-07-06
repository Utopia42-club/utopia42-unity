using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Snack
{
    public class Toast : UxmlElement
    {
        public Toast(string message, ToastType type) : base("Ui/Snack/Toast")
        {
            var msg = this.Q<Label>("message");
            msg.text = message;
            var icon = this.Q<VisualElement>("icon");
            UiImageUtils.SetBackground(icon, type switch
            {
                ToastType.Info => Resources.Load<Sprite>("Icons/info"),
                ToastType.Warning => Resources.Load<Sprite>("Icons/warning"),
                ToastType.Error => Resources.Load<Sprite>("Icons/error"),
                _ => Resources.Load<Sprite>("Icons/info")
            }, false);
        }

        public SnackController Show()
        {
            return SnackService.INSTANCE.Show(
                new SnackConfig(this)
            );
        }
        
        public SnackController ShowWithCloseButtonDisabled()
        {
            return SnackService.INSTANCE.Show(
                new SnackConfig(this).WithCloseButtonVisible(false)
            );
        }

        public enum ToastType
        {
            Error,
            Warning,
            Info
        }
    }
}