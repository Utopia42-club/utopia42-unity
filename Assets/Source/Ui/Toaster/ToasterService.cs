using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Toaster
{
    public class ToasterService
    {
        private static ToasterService instance;

        public static ToasterService Instance => instance ??= new ToasterService();

        public static void Show(string message, ToastType type, long? duration = 3)
        {
            var toast = Utils.Utils.Create("Ui/Toaster/Toast");
            var panel = toast.Q<VisualElement>("panel");
            var msg = toast.Q<Label>("message");
            msg.text = message;
            var icon = toast.Q<Image>("icon");
            icon.sprite = type switch
            {
                ToastType.Info => Resources.Load<Sprite>("Icons/info"),
                ToastType.Warning => Resources.Load<Sprite>("Icons/warning"),
                ToastType.Error => Resources.Load<Sprite>("Icons/error"),
                _ => Resources.Load<Sprite>("Icons/info")
            };
            panel.style.width = msg.text.Length * 10 + 70;

            if (duration.HasValue)
            {
                var closeCoroutine = CloseToast(toast, duration.Value);
                toast.RegisterCallback<MouseEnterEvent>(evt => ToastLayer.INSTANCE.StopCoroutine(closeCoroutine));
                toast.RegisterCallback<MouseLeaveEvent>(evt => ToastLayer.INSTANCE.StartCoroutine(closeCoroutine));
                ToastLayer.INSTANCE.StartCoroutine(closeCoroutine);
            }
            else
            {
                toast.Q<Button>("close").clickable.clicked += () => toast.RemoveFromHierarchy();
            }

            ToastLayer.INSTANCE.VisualElement().Add(toast);
        }

        private static IEnumerator CloseToast(VisualElement toast, long duration)
        {
            yield return new WaitForSeconds(duration);
            toast.RemoveFromHierarchy();
        }

        public enum ToastType
        {
            Error,
            Warning,
            Info
        }
    }
}