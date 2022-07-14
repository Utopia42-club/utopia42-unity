using System;
using JetBrains.Annotations;

namespace Source.Ui.Dialog
{
    public class DialogAction
    {
        private string text;
        private Action action;
        [CanBeNull] private string styleClass;
        private readonly bool closeOnPerform;

        public DialogAction(string text, Action action, [CanBeNull] string styleClass = null, bool closeOnPerform = true)
        {
            this.text = text;
            this.action = action;
            this.styleClass = styleClass;
            this.closeOnPerform = closeOnPerform;
        }

        public string Text => text;

        public Action Action => action;

        [CanBeNull] public string StyleClass => styleClass;

        public bool CloseOnPerform => closeOnPerform;
    }
}