using System.Collections;
using System.Collections.Generic;
using Source.Canvas;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public class ToolTipManipulator : Manipulator
    {
        private VisualElement element;
        private readonly VisualElement rootElement;
        private static VisualElement currentTooltip;

        public ToolTipManipulator(VisualElement rootElement)
        {
            this.rootElement = rootElement;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseIn);
            target.RegisterCallback<MouseOutEvent>(MouseOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseIn);
            target.UnregisterCallback<MouseOutEvent>(MouseOut);
        }

        private void MouseIn(MouseEnterEvent e)
        {
            if (string.IsNullOrEmpty(target.tooltip))
                return;
            currentTooltip?.RemoveFromHierarchy();
            var label = new Label(target.tooltip)
            {
                style =
                {
                    color = Color.white
                }
            };

            var left = target.worldBound.xMin + target.worldBound.width / 2;
            var top = target.worldBound.yMax + 5;
            element = new VisualElement
            {
                style =
                {
                    backgroundColor = Colors.PRIMARY_COLOR,
                    position = Position.Absolute,
                    left = left,
                    top = top,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderTopRightRadius = 8,
                    borderTopLeftRadius = 8,
                    transitionDelay = new List<TimeValue> {new(50, TimeUnit.Millisecond)},
                    transitionDuration = new List<TimeValue> {new(100, TimeUnit.Millisecond)},
                }
            };
            element.Add(label);
            rootElement.Add(element);
            element.BringToFront();
            currentTooltip = element;

            element.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var width = element.worldBound.width;
                while (left + width > rootElement.worldBound.xMax)
                    left -= 1;
                element.style.left = left;
            });
        }

        private void MouseOut(MouseOutEvent e)
        {
            Destroy();
        }

        public void Destroy()
        {
            element?.RemoveFromHierarchy();
            currentTooltip = null;
        }
    }
}