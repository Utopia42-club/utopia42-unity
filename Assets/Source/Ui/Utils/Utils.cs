using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public static class Utils
    {
        [Obsolete("Create is deprecated, please use UxmlElement class.")]
        public static VisualElement Create(string uxmlPath)
        {
            return Resources.Load<VisualTreeAsset>(uxmlPath).CloneTree();
        }
    }
}