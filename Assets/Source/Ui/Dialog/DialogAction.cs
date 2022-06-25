using System;
using JetBrains.Annotations;

namespace Source.Ui.Dialog
{
    public class DialogAction
    {
        private string text;
        private Action action;
        [CanBeNull] private string styleClass;

        public DialogAction(string text, Action action, [CanBeNull] string styleClass = null)
        {
            this.text = text;
            this.action = action;
            this.styleClass = styleClass;
        }

        public string Text => text;

        public Action Action => action;

        [CanBeNull] public string StyleClass => styleClass;
    }
}