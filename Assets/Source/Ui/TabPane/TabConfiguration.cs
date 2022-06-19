using System;

namespace Source.Ui.TabPane
{
    public class TabConfiguration
    {
        public string name { get; set; }
        public string uxmlPath { get; set; }
        public Action onTabOpen { get; set; }

        public TabConfiguration(string name, string uxmlPath, Action onTabOpen)
        {
            this.name = name;
            this.uxmlPath = uxmlPath;
            this.onTabOpen = onTabOpen;
        }
    }
}