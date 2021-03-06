using Source.Ui.Popup;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Profile
{
    public class SocialLink : VisualElement
    {
        public SocialLink(Model.Profile.Link link)
        {
            AddToClassList("social-link");
            var icon = new VisualElement();
            icon.AddToClassList("social-link__icon");
            UiImageUtils.SetBackground(icon, link.GetMedia().GetIcon(), false);
            Add(icon);

            var label = new Label();
            label.AddToClassList("social-link__label");
            label.text = link.GetMedia().GetName();
            if (link.link != null)
            {
                label.RegisterCallback<MouseDownEvent>(evt => Application.OpenURL(link.link));
                label.tooltip = link.link;
                label.AddManipulator(new ToolTipManipulator(Side.TopRight));
            }

            Add(label);

            style.alignItems = Align.Center;
        }
    }
}