using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace Source.Ui.Snack
{
    public class SnackConfig
    {
        private StyleLength width = new Length(100, LengthUnit.Percent);
        private StyleLength height = StyleKeyword.Auto;
        private VisualElement content;
        [CanBeNull] private string title;
        [CanBeNull] private int? duration;
        private bool closeButtonVisible = true;
        private readonly Side verticalSide;
        private readonly Side horizontalSide;

        public SnackConfig(VisualElement content,
            Side verticalSide = Side.End,
            Side horizontalSide = Side.End,
            int? duration = 4)
        {
            this.content = content;
            this.verticalSide = verticalSide;
            this.horizontalSide = horizontalSide;
            this.duration = duration;
        }

        public SnackConfig WithWidth(StyleLength width)
        {
            this.width = width;
            return this;
        }

        public SnackConfig WithHeight(StyleLength height)
        {
            this.height = height;
            return this;
        }

        public SnackConfig WithTitle(string title)
        {
            this.title = title;
            return this;
        }

        public SnackConfig WithCloseButtonVisible(bool b)
        {
            closeButtonVisible = b;
            return this;
        }

        public StyleLength Width => width;

        public StyleLength Height => height;

        public VisualElement Content => content;

        [CanBeNull] public string Title => title;

        public int? Duration => duration;

        public bool CloseButtonVisible => closeButtonVisible;

        public Side VerticalSide => verticalSide;

        public Side HorizontalSide => horizontalSide;

        public enum Side
        {
            Start,
            Middle,
            End,
        }
    }
}