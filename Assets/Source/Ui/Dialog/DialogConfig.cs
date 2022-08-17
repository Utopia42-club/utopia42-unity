using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace Source.Ui.Dialog
{
    public class DialogConfig
    {
        private StyleLength width = StyleKeyword.Auto;
        private StyleLength height = StyleKeyword.Auto;
        private VisualElement content;
        [CanBeNull] private Action onClose;
        [CanBeNull] private string title;
        private List<DialogAction> actions = new();
        private bool closeOnBackdropClick = true;

        public DialogConfig(VisualElement content)
        {
            title = "";
            this.content = content;
        }

        public DialogConfig(string title, VisualElement content)
        {
            this.title = title;
            this.content = content;
        }

        public DialogConfig WithWidth(StyleLength width)
        {
            this.width = width;
            return this;
        }

        public DialogConfig WithHeight(StyleLength height)
        {
            this.height = height;
            return this;
        }

        public DialogConfig WithAction(DialogAction action)
        {
            actions.Add(action);
            return this;
        }

        public DialogConfig WithOnClose(Action onClose)
        {
            this.onClose = onClose;
            return this;
        }

        public DialogConfig WithCancelAction(Action action = null)
        {
            actions.Add(new DialogAction("Cancel", action ?? (() => { })));
            return this;
        }

        public DialogConfig WithCloseOnBackdropClick(bool value = true)
        {
            closeOnBackdropClick = value;
            return this;
        }

        public StyleLength Width => width;

        public StyleLength Height => height;

        public VisualElement Content => content;

        [CanBeNull] public string Title => title;

        public List<DialogAction> Actions => actions;
        public bool CloseOnBackdropClick => closeOnBackdropClick;

        [CanBeNull] public Action OnClose => onClose;
    }
}