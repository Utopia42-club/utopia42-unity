using System;
using UnityEngine.UIElements;

namespace Source.Ui.TabPane
{
    public class TabConfiguration
    {
        public string name { get; set; }

        public VisualElement VisualElement { get; set; }
        public Action<TabOpenEvent> onTabOpen { get; set; }

        public Func<VisualElement> visualElementFactory { get; set; }

        public Action<TabCloseEvent> onTabClose { get; set; }

        public TabConfiguration(string name, string uxmlPath, Action<TabOpenEvent> onTabOpen = null,
            Action<TabCloseEvent> onTabClose = null)
            : this(name, Utils.Utils.Create(uxmlPath), onTabOpen, onTabClose)
        {
        }

        public TabConfiguration(string name, VisualElement visualElement, Action<TabOpenEvent> onTabOpen = null,
            Action<TabCloseEvent> onTabClose = null)
            : this(name, onTabOpen, onTabClose)
        {
            VisualElement = visualElement;
        }

        public TabConfiguration(string name, Func<VisualElement> visualElementFactory,
            Action<TabOpenEvent> onTabOpen = null,
            Action<TabCloseEvent> onTabClose = null) : this(name, onTabOpen, onTabClose)
        {
            this.visualElementFactory = visualElementFactory;
        }

        private TabConfiguration(string name, Action<TabOpenEvent> onTabOpen = null,
            Action<TabCloseEvent> onTabClose = null)
        {
            this.name = name;
            this.onTabClose = onTabClose;
            this.onTabOpen = onTabOpen;
        }
    }
}