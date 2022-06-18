using UnityEngine;
using UnityEngine.UIElements;

namespace src.Ui
{
    public class UxmlElement : VisualElement
    {
        public UxmlElement(string resourcePath)
        {
            Resources.Load<VisualTreeAsset>(resourcePath).CloneTree(this);
            style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        }
    }
}