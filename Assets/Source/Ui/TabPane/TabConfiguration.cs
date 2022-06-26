using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.TabPane
{
    public class TabConfiguration
    {
        public string name { get; set; }

        public VisualElement VisualElement { get; set; }

        public Func<VisualElement> visualElementFactory { get; set; }

        public Action onTabOpen { get; set; }

        public Action onTabClose { get; set; }

        public TabConfiguration(string name, string uxmlPath, Action onTabOpen = null, Action onTabClose = null) :
            this(name, () => Utils.Utils.Create(uxmlPath), onTabOpen, onTabClose)
        {
        }

        public TabConfiguration(string name, VisualElement visualElement, Action onTabOpen = null,
            Action onTabClose = null) : this(name, onTabOpen, onTabClose)
        {
            VisualElement = visualElement;
        }

        public TabConfiguration(string name, Func<VisualElement> visualElementFactory, Action onTabOpen = null,
            Action onTabClose = null) : this(name, onTabOpen, onTabClose)
        {
            this.visualElementFactory = visualElementFactory;
        }

        private TabConfiguration(string name, Action onTabOpen = null, Action onTabClose = null)
        {
            this.name = name;
            this.onTabOpen = onTabOpen;
            this.onTabClose = onTabClose;
        }
    }
}