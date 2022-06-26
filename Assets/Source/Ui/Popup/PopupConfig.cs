using System;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace Source.Ui.Popup
{
    public class PopupConfig
    {
        private StyleLength width = StyleKeyword.Auto;
        private StyleLength height = StyleKeyword.Auto;
        private VisualElement content;
        private VisualElement target;
        [CanBeNull] private Action onClose;
        private readonly Side side;
        private bool backdropLayer = true;

        public PopupConfig(VisualElement content, VisualElement target, Side side)
        {
            this.content = content;
            this.target = target;
            this.side = side;
        }

        public PopupConfig WithWidth(StyleLength width)
        {
            this.width = width;
            return this;
        }

        public PopupConfig WithHeight(StyleLength height)
        {
            this.height = height;
            return this;
        }


        public PopupConfig WithOnClose(Action onClose)
        {
            this.onClose = onClose;
            return this;
        }

        public PopupConfig WithBackDropLayer(bool b)
        {
            backdropLayer = b;
            return this;
        }

        public bool BackdropLayer => backdropLayer;

        public StyleLength Width => width;

        public StyleLength Height => height;

        public VisualElement Content => content;

        [CanBeNull] public Action OnClose => onClose;

        public VisualElement Target => target;

        public Side Side => side;
    }

    public enum Side
    {
        Top,
        TopLeft,
        TopRight,
        Bottom,
        BottomLeft,
        BottomRight,
    }
}