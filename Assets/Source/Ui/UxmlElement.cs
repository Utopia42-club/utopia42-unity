using System;
using Siccity.GLTFUtility;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui
{
    public class UxmlElement : VisualElement
    {
        public UxmlElement()
            : this(null)
        {
        }

        public UxmlElement(string resourcePath)
            : this(resourcePath, false)
        {
        }

        public UxmlElement(bool fillSize)
            : this(null, fillSize)
        {
        }

        public UxmlElement(string resourcePath, bool fillSize)
            : this(resourcePath, new StyleLength(new Length(100, LengthUnit.Percent)),
                new StyleLength(new Length(100, LengthUnit.Percent)))
        {
        }

        public UxmlElement(StyleLength width, StyleLength height)
            : this(null, true, width, height)
        {
        }

        public UxmlElement(string resourcePath, StyleLength width, StyleLength height)
            : this(resourcePath, true, width, height)
        {
        }

        private UxmlElement(string resourcePath, bool setSize, StyleLength width, StyleLength height)
        {
            if (resourcePath == null)
            {
                var fullName = GetType().FullName;
                var parts = fullName?.Split(".");
                if (parts == null || parts.Length <= 1)
                    throw new ArgumentException("Invalid class fullname: " + fullName);
                resourcePath = string.Join("/", parts.SubArray(1, parts.Length - 1));
            }

            Resources.Load<VisualTreeAsset>(resourcePath).CloneTree(this);
            if (setSize)
            {
                style.width = width;
                style.height = height;
            }
        }
    }
}