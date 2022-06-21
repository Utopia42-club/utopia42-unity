using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.TabPane
{
    public class TabConfiguration
    {
        public string name { get; set; }

        public VisualElement VisualElement { get; set; }
        public Action onTabOpen { get; set; }

        public TabConfiguration(string name, string uxmlPath, Action onTabOpen) :
            this(name, Utils.Utils.Create(uxmlPath), onTabOpen)
        {
        }

        public TabConfiguration(string name, VisualElement visualElement, Action onTabOpen)
        {
            this.name = name;
            VisualElement = visualElement;
            this.onTabOpen = onTabOpen;
        }
    }
}