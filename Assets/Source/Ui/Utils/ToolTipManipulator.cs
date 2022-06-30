using Source.Ui.Popup;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public class ToolTipManipulator : Manipulator
    {
        private VisualElement element;
        private int? popupId;
        private readonly Side side;

        public ToolTipManipulator(Side side = Side.BottomRight)
        {
            this.side = side;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseIn);
            target.RegisterCallback<MouseLeaveEvent>(MouseLeave, TrickleDown.TrickleDown);
            target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseIn);
            target.UnregisterCallback<MouseLeaveEvent>(MouseLeave);
            target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void MouseIn(MouseEnterEvent e)
        {
            if (string.IsNullOrEmpty(target.tooltip) || popupId != null)
                return;
            var label = new Label(target.tooltip)
            {
                style =
                {
                    color = Color.white
                }
            };
            popupId = PopupService.INSTANCE.Show(new PopupConfig(label, target, side).WithBackDropLayer(false));
        }

        private void MouseLeave(MouseLeaveEvent evt)
        {
            Destroy();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Destroy();
        }

        public void Destroy()
        {
            if (popupId != null)
            {
                PopupService.INSTANCE.Close(popupId.Value);
                popupId = null;
            }
        }
    }
}