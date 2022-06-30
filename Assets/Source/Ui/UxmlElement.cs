using System;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui
{
    public class UxmlElement : VisualElement
    {
        public UxmlElement(Type type)
            : this(ResourcePaths.ForType(type))
        {
        }

        public UxmlElement(string resourcePath)
            : this(resourcePath, false)
        {
        }

        public UxmlElement(Type type, bool fillSize)
            : this(ResourcePaths.ForType(type), fillSize)
        {
        }

        public UxmlElement(string resourcePath, bool fillSize)
            : this(resourcePath, fillSize, new StyleLength(new Length(100, LengthUnit.Percent)),
                new StyleLength(new Length(100, LengthUnit.Percent)))
        {
        }

        public UxmlElement(Type type, StyleLength width, StyleLength height)
            : this(ResourcePaths.ForType(type), true, width, height)
        {
        }

        public UxmlElement(string resourcePath, StyleLength width, StyleLength height)
            : this(resourcePath, true, width, height)
        {
        }

        private UxmlElement(string resourcePath, bool setSize, StyleLength width, StyleLength height)
        {
            var resource = Resources.Load<VisualTreeAsset>(resourcePath);
            if (resource == null) throw new ArgumentException("Could not load uxml resource: " + resourcePath);
            resource.CloneTree(this);
            if (setSize)
            {
                style.width = width;
                style.height = height;
            }
        }
        
        public static StyleSheet LoadStyleSheet(Type type)
        {
            return LoadStyleSheet(ResourcePaths.ForType(type));
        }

        public static StyleSheet LoadStyleSheet(string path)
        {
            return Resources.Load<StyleSheet>(path);
        }
    }
}