using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Source.Ui.Dialog;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Snack
{
    public class SnackService : MonoBehaviour
    {
        private static SnackService instance;
        public static SnackService INSTANCE => instance;

        private static int snackId = 0;
        private static Dictionary<int, VisualElement> snacks = new();
        private VisualElement root;
        private VisualElement topLeft;
        private VisualElement topMiddle;
        private VisualElement topRight;
        private VisualElement middleLeft;
        private VisualElement middleMiddle;
        private VisualElement middleRight;
        private VisualElement bottomLeft;
        private VisualElement bottomMiddle;
        private VisualElement bottomRight;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            instance = this;
            root = GetComponent<UIDocument>().rootVisualElement;
            topLeft = root.Q<VisualElement>("topLeft");
            topMiddle = root.Q<VisualElement>("topMiddle");
            topRight = root.Q<VisualElement>("topRight");
            middleLeft = root.Q<VisualElement>("middleLeft");
            middleMiddle = root.Q<VisualElement>("middleMiddle");
            middleRight = root.Q<VisualElement>("middleRight");
            bottomLeft = root.Q<VisualElement>("bottomLeft");
            bottomMiddle = root.Q<VisualElement>("bottomMiddle");
            bottomRight = root.Q<VisualElement>("bottomRight");
        }

        public SnackController Show(SnackConfig config)
        {
            if (snacks.Count == 0)
                gameObject.SetActive(true);

            var id = snackId++;
            var snack = new Snack(config, id)
            {
                style =
                {
                    width = config.Width,
                    height = config.Height
                }
            };
            UpdateSnackPosition(config, snack);
            snack.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
            snacks.Add(id, snack);

            var controller = new SnackController(snack, id);

            if (config.Duration.HasValue)
            {
                var closeCoroutine = CloseCoroutine(controller, config.Duration.Value);
                snack.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    StopCoroutine(closeCoroutine);
                });
                snack.RegisterCallback<MouseLeaveEvent>(evt => StartCoroutine(closeCoroutine));
                StartCoroutine(closeCoroutine);
            }

            return controller;
        }

        private IEnumerator CloseCoroutine(SnackController controller, int duration)
        {
            yield return new WaitForSeconds(duration);
            controller.Close();
        }

        private void UpdateSnackPosition(SnackConfig config, Snack snack)
        {
            switch (config.VerticalSide)
            {
                case SnackConfig.Side.Start:
                    switch (config.HorizontalSide)
                    {
                        case SnackConfig.Side.Start:
                            topLeft.Add(snack);
                            break;
                        case SnackConfig.Side.Middle:
                            topMiddle.Add(snack);
                            break;
                        case SnackConfig.Side.End:
                            topRight.Add(snack);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case SnackConfig.Side.Middle:
                    switch (config.HorizontalSide)
                    {
                        case SnackConfig.Side.Start:
                            middleLeft.Add(snack);
                            break;
                        case SnackConfig.Side.Middle:
                            middleMiddle.Add(snack);
                            break;
                        case SnackConfig.Side.End:
                            middleRight.Add(snack);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case SnackConfig.Side.End:
                    switch (config.HorizontalSide)
                    {
                        case SnackConfig.Side.Start:
                            bottomLeft.Add(snack);
                            break;
                        case SnackConfig.Side.Middle:
                            bottomMiddle.Add(snack);
                            break;
                        case SnackConfig.Side.End:
                            bottomRight.Add(snack);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Close(int id)
        {
            if (snacks.ContainsKey(id))
            {
                (snacks[id].userData as DialogConfig)?.OnClose?.Invoke();
                snacks[id].SetEnabled(false);
                snacks[id].RemoveFromHierarchy();
                snacks.Remove(id);
                if (snacks.Count == 0)
                    gameObject.SetActive(false);
            }
        }

        public void CloseAll()
        {
            foreach (var id in snacks.Select(pair => pair.Key).ToList())
                Close(id);
        }
    }
}