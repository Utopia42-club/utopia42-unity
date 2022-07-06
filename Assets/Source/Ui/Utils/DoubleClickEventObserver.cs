using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public class DoubleClickEventObserver
    {
        private readonly float epsilonTime = 0.2f;
        private readonly float epsilonDistance = 4;

        private readonly VisualElement target;
        private readonly EventCallback<MouseDownEvent> mouseDownListener;
        private readonly EventCallback<MouseUpEvent> mouseUpListener;

        private int clickCount = 0;
        private bool mouseDown = false;
        private float? lastTime = null;
        private Vector2? lastPosition = null;


        public DoubleClickEventObserver(VisualElement target, Action<MouseUpEvent> callBack)
        {
            this.target = target;

            mouseDownListener = e =>
            {
                if (mouseDown)
                    ResetState();
                UpdateTime();
                UpdatePosition(e.mousePosition);

                mouseDown = true;
            };
            target.RegisterCallback(mouseDownListener);

            mouseUpListener = e =>
            {
                UpdateTime();
                UpdatePosition(e.mousePosition);
                if (!mouseDown)
                {
                    ResetState();
                    return;
                }

                clickCount++;
                mouseDown = false;
                if (clickCount == 2)
                {
                    callBack.Invoke(e);
                    ResetState();
                }
            };
            target.RegisterCallback(mouseUpListener);
        }

        private void UpdatePosition(Vector2 newPos)
        {
            if (lastPosition.HasValue && (newPos - lastPosition.Value).magnitude >= epsilonDistance)
            {
                ResetState();
                return;
            }

            lastPosition = newPos;
        }

        private void UpdateTime()
        {
            if (lastTime.HasValue &&
                Time.realtimeSinceStartup - lastTime.Value >= Math.Max(epsilonTime, Time.deltaTime))
            {
                ResetState();
                return;
            }

            lastTime = Time.realtimeSinceStartup;
        }

        private void ResetState()
        {
            lastTime = null;
            lastPosition = null;
            clickCount = 0;
            mouseDown = false;
        }

        public void Detach()
        {
            target.UnregisterCallback(mouseDownListener);
            target.UnregisterCallback(mouseUpListener);
        }
    }
}