using System;
using UnityEngine.UIElements;

namespace Source.Ui.TabPane
{
    public class TabConfiguration
    {
        public string name { get; set; }

        public VisualElement VisualElement { get; set; }
        public Action<TabOpenEvent> onTabOpen { get; set; }

        public TabConfiguration(string name, string uxmlPath, Action<TabOpenEvent> onTabOpen) :
            this(name, Utils.Utils.Create(uxmlPath), onTabOpen)
        {
        }

        public TabConfiguration(string name, VisualElement visualElement, Action<TabOpenEvent> onTabOpen = null)
        {
            this.name = name;
            VisualElement = visualElement;
            this.onTabOpen = (e) =>
            {
                if (visualElement is TabOpenListener)
                    ((TabOpenListener) visualElement).OnTabOpen(e);
                onTabOpen?.Invoke(e);
            };
        }
    }
}