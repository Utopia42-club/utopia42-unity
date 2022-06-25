using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Popup
{
    public class PopupService : MonoBehaviour
    {
        private static PopupService instance;
        public static PopupService INSTANCE => instance;

        private static int popupId = 0;
        private static readonly Dictionary<int, VisualElement> backDroppedPopups = new();
        private static Dictionary<int, VisualElement> nonBackDroppedPopups = new();
        private VisualElement root;
        private VisualElement popupLayer;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            instance = this;
            root = GetComponent<UIDocument>().rootVisualElement;
            popupLayer = root.Q<VisualElement>("layer");
            popupLayer.RegisterCallback<MouseDownEvent>(evt => Close(backDroppedPopups.Last().Key));
        }

        public int Show(PopupConfig config)
        {
            return Show(config, out _);
        }

        public int Show(PopupConfig config, out VisualElement popup)
        {
            if (backDroppedPopups.Count == 0 && nonBackDroppedPopups.Count == 0)
                gameObject.SetActive(true);
            popup = Utils.Utils.Create("Ui/Popup/Popup");
            popup.style.width = config.Width;
            popup.style.height = config.Height;
            popup.style.position = Position.Absolute;
            popup.style.transitionDelay = new List<TimeValue> {new(50, TimeUnit.Millisecond)};
            popup.style.transitionDuration = new List<TimeValue> {new(100, TimeUnit.Millisecond)};

            var element = popup;
            popup.RegisterCallback<GeometryChangedEvent>(_ => UpdatePopupPosition(config, element));

            var content = popup.Q<VisualElement>("popupContent");
            content.Add(config.Content);

            var id = popupId++;
            popup.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
            popup.userData = config;
            
            if (config.BackdropLayer)
            {
                popupLayer.style.display = DisplayStyle.Flex;
                popupLayer.Add(popup);
                backDroppedPopups.Add(id, popup);
            }
            else
            {
                popupLayer.style.display = DisplayStyle.None;
                root.Add(popup);
                nonBackDroppedPopups.Add(id, popup);
            }

            return id;
        }

        private void UpdatePopupPosition(PopupConfig config, VisualElement element)
        {
            float left;
            float top;
            var width = element.worldBound.width;
            switch (config.Side)
            {
                case Side.TopLeft:
                    left = config.Target.worldBound.xMin + config.Target.worldBound.width / 2 - width;
                    top = config.Target.worldBound.yMin - 5 - config.Target.worldBound.height;
                    while (left - width < root.worldBound.xMin)
                        left += 1;
                    break;
                case Side.TopRight:
                    left = config.Target.worldBound.xMin + config.Target.worldBound.width / 2;
                    top = config.Target.worldBound.yMin - 5 - config.Target.worldBound.height;
                    while (left + width > root.worldBound.xMax)
                        left -= 1;
                    break;
                case Side.BottomLeft:
                    left = config.Target.worldBound.xMin + config.Target.worldBound.width / 2 - width;
                    top = config.Target.worldBound.yMax + 5;
                    while (left - width < root.worldBound.xMin)
                        left += 1;
                    break;
                case Side.BottomRight:
                    left = config.Target.worldBound.xMin + config.Target.worldBound.width / 2;
                    top = config.Target.worldBound.yMax + 5;
                    while (left + width > root.worldBound.xMax)
                        left -= 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            element.style.left = left;
            element.style.top = top;
        }

        public void Close(int id)
        {
            if (backDroppedPopups.ContainsKey(id))
            {
                (backDroppedPopups[id].userData as PopupConfig)?.OnClose?.Invoke();
                backDroppedPopups[id].RemoveFromHierarchy();
                backDroppedPopups.Remove(id);
            }
            else if (nonBackDroppedPopups.ContainsKey(id))
            {
                (nonBackDroppedPopups[id].userData as PopupConfig)?.OnClose?.Invoke();
                nonBackDroppedPopups[id].RemoveFromHierarchy();
                nonBackDroppedPopups.Remove(id);
            }

            if (backDroppedPopups.Count == 0 && nonBackDroppedPopups.Count == 0)
                gameObject.SetActive(false);
        }
    }
}