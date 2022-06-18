using UnityEngine;
using UnityEngine.UIElements;

namespace src.Ui
{
    public class UxmlElement : VisualElement
    {
        public UxmlElement(string resourcePath)
            : this(resourcePath, false)
        {
        }

        public UxmlElement(string resourcePath, bool fillSize)
            : this(resourcePath, new StyleLength(new Length(100, LengthUnit.Percent)),
                new StyleLength(new Length(100, LengthUnit.Percent)))
        {
        }

        public UxmlElement(string resourcePath, StyleLength width, StyleLength height)
            : this(resourcePath, true, width, height)
        {
        }

        private UxmlElement(string resourcePath, bool setSize, StyleLength width, StyleLength height)
        {
            Resources.Load<VisualTreeAsset>(resourcePath).CloneTree(this);
            if (setSize)
            {
                style.width = width;
                style.height = height;
            }
        }
    }
}