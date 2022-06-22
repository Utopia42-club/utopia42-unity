using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class SocialLink : VisualElement
    {
        public SocialLink(Model.Profile.Link link)
        {
            AddToClassList("social-link");  
            var icon = new VisualElement();
            icon.AddToClassList("social-link__icon");
            UiImageLoader.SetBackground(icon, link.GetMedia().GetIcon());
            Add(icon);

            var label = new Label();
            label.AddToClassList("social-link__label");
            label.text = link.GetMedia().GetName();
            label.RegisterCallback<MouseDownEvent>(evt => Application.OpenURL(link.link));
            Add(label);
        }
    }
}