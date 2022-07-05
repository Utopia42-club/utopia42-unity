using System.Collections.Generic;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

[Preserve]
public class Shortcut : VisualElement
{
    private readonly Label label;

    [Preserve]
    public new class UxmlFactory : UxmlFactory<Shortcut, UxmlTraits>
    {
    }

    [Preserve]
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlStringAttributeDescription value = new()
            {name = "value", defaultValue = ""};

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }


        public override void Init(VisualElement visualElement, IUxmlAttributes attributes,
            CreationContext creationContext)
        {
            base.Init(visualElement, attributes, creationContext);
            if (visualElement is Shortcut element)
            {
                element.label.text = value.GetValueFromBag(attributes, creationContext);
                element.style.width = element.label.text.Length * 10 + 30;
            }
        }
    }


    public Shortcut()
    {
        style.width = 40;
        style.height = 40;
        style.alignContent = Align.Center;
        style.alignItems = Align.Center;
        style.justifyContent = Justify.Center;
        UiImageUtils.SetBackground(this, Resources.Load<Sprite>("Icons/keyboard_key_empty"), false,
            ScaleMode.StretchToFill);
        label = new Label
        {
            style =
            {
                fontSize = 14,
                unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                marginBottom = 0,
                marginLeft = 0,
                marginRight = 0,
                marginTop = 0,
                paddingBottom = 0,
                paddingLeft = 0,
                paddingRight = 0,
                paddingTop = 0
            }
        };
        AddToClassList("utopia-background-tint-color");
        Add(label);
    }
}