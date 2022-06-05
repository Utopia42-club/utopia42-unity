using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.Canvas
{
    public class ToolTipManipulator : Manipulator
    {
        private VisualElement element;
        private readonly VisualElement rootElement;

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
            element = new VisualElement
            {
                style =
                {
                    backgroundColor = Colors.PRIMARY_COLOR,
                    position = Position.Absolute,
                    left = target.worldBound.xMin + 10,
                    top = target.worldBound.yMax + 5,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderTopRightRadius = 8,
                    borderTopLeftRadius = 8,
                }
            };
            element.style.transitionDelay = new List<TimeValue> {new(200, TimeUnit.Millisecond)};
            element.style.transitionDuration = new List<TimeValue> {new(50, TimeUnit.Millisecond)};
            var label = new Label(target.tooltip)
            {
                style =
                {
                    color = Color.white
                }
            };
            element.Add(label);
            rootElement.Add(element);
            element.BringToFront();
        }

        private void MouseOut(MouseOutEvent e)
        {
            element.RemoveFromHierarchy();
        }
    }
}