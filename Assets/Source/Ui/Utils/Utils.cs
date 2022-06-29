using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public static class Utils
    {
        [Obsolete("Create is deprecated, please use UxmlElement class.")]
        public static VisualElement Create(string uxmlPath)
        {
            return Resources.Load<VisualTreeAsset>(uxmlPath).CloneTree();
        }

        public static void RegisterOnDoubleClick(VisualElement visualElement, Action<MouseDownEvent> action)
        {
            var clicked = 0;
            float clickTime = 0;
            const float clickDelay = 0.5f;
            visualElement.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != (int) MouseButton.LeftMouse)
                    return;
                clicked++;
                if (clicked == 1)
                    clickTime = Time.time;
                else if (clicked > 1)
                {
                    if (Time.time - clickTime < clickDelay)
                    {
                        clicked = 0;
                        clickTime = 0;
                        action.Invoke(evt);
                    }
                    else
                    {
                        clicked = 1;
                        clickTime = Time.time;
                    }
                }
            });
        }
    }
}