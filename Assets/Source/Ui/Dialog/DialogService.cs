using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Dialog
{
    public class DialogService : MonoBehaviour
    {
        private static DialogService instance;
        public static DialogService INSTANCE => instance;

        private static int dialogId = 0;
        private static Dictionary<int, VisualElement> dialogs = new();
        private VisualElement root;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            instance = this;
            root = GetComponent<UIDocument>().rootVisualElement;
        }

        public void CloseLastOpenedDialog()
        {
            Close(dialogs.Last().Key);
        }

        public bool IsAnyDialogOpen()
        {
            return dialogs.Count > 0;
        }

        public int Show(DialogConfig config)
        {
            return Show(config, out _);
        }

        public int Show(DialogConfig config, out VisualElement dialog)
        {
            if (dialogs.Count == 0)
                gameObject.SetActive(true);
            var dialogContainer = Utils.Utils.Create("Ui/Dialog/Dialog");
            dialogContainer.style.width =
                dialogContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            dialogContainer.style.position = Position.Absolute;
            dialog = dialogContainer.Q<VisualElement>("dialog");
            dialog.style.width = config.Width;
            dialog.style.height = config.Height;
            dialog.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
            var dialogLayer = dialogContainer.Q<VisualElement>("layer");
            dialogLayer.RegisterCallback<MouseDownEvent>(_ => CloseLastOpenedDialog());

            if (config.Title != null)
            {
                var title = dialog.Q<Label>("dialogTitle");
                title.text = config.Title;
            }

            var content = dialog.Q<VisualElement>("dialogContent");
            content.Add(config.Content);

            var actions = dialog.Q<VisualElement>("dialogActions");

            var id = dialogId++;

            if (config.Actions.Count > 0)
            {
                foreach (var action in config.Actions)
                {
                    var button = new Button
                    {
                        text = action.Text
                    };
                    button.clickable.clicked += () =>
                    {
                        if (action.CloseOnPerform)
                            Close(id);
                        action.Action();
                    };
                    if (action.StyleClass != null)
                        button.AddToClassList(action.StyleClass);
                    else
                        button.AddToClassList("utopia-stroked-button-primary");
                    actions.Add(button);
                }
            }
            else
            {
                actions.style.display = DisplayStyle.None;
            }

            var closeAction = dialogContainer.Q<Button>("dialogCloseAction");
            closeAction.clickable.clicked += () => Close(id);
            dialogContainer.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
            dialogContainer.userData = config;
            dialogs.Add(id, dialogContainer);
            root.Add(dialogContainer);
            return id;
        }

        public void Close(int id)
        {
            if (dialogs.ContainsKey(id))
            {
                (dialogs[id].userData as DialogConfig)?.OnClose?.Invoke();
                dialogs[id].SetEnabled(false);
                dialogs[id].RemoveFromHierarchy();
                dialogs.Remove(id);
                if (dialogs.Count == 0)
                    gameObject.SetActive(false);
            }
        }

        public void CloseAll()
        {
            foreach (var id in dialogs.Select(pair => pair.Key).ToList())
                Close(id);
        }
    }
}