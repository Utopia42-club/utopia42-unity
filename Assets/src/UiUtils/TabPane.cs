using System.Collections.Generic;
using src.AssetsInventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.UiUtils
{
    public class TabPane
    {
        private readonly TemplateContainer root;
        private readonly List<TabConfiguration> tabConfigs;
        private readonly List<Button> tabButtons = new();
        private int currentTab;
        private readonly VisualElement tabBody;
        private readonly VisualElement tabButtonsArea;
        private readonly VisualElement breadcrumb;
        
        public TabPane(List<TabConfiguration> tabConfigs)
        {
            this.tabConfigs = tabConfigs;
            root = Resources.Load<VisualTreeAsset>("UiDocuments/TabPane").CloneTree();
            tabBody = root.Q<VisualElement>("tabBody");
            breadcrumb = root.Q<VisualElement>("breadcrumb");
            tabButtonsArea = root.Q<VisualElement>("tabs");
            for (var ind = 0; ind < tabConfigs.Count; ind++)
            {
                var tabConfig = tabConfigs[ind];
                var button = new Button
                {
                    text = tabConfig.name
                };
                button.AddToClassList("tab-button");
                var i = ind;
                button.clickable.clicked += () => OpenTab(i);
                tabButtons.Add(button);
                tabButtonsArea.Add(button);
            }
        }

        public void OpenTab(int index)
        {
            breadcrumb.style.display = DisplayStyle.None;
            var config = tabConfigs[index];
            var tabBodyContent = Resources.Load<VisualTreeAsset>(config.uxmlPath).CloneTree();
            tabBodyContent.style.width = new StyleLength(new Length(95, LengthUnit.Percent));
            tabBody.Clear();
            tabBody.Add(tabBodyContent);
            foreach (var button in tabButtons)
                button.RemoveFromClassList("selected-tab");
            tabButtons[index].AddToClassList("selected-tab");
            currentTab = index;
            config.onTabOpen.Invoke();
        }

        public VisualElement GetTabBody()
        {
            return tabBody;
        }

        public int GetCurrentTab()
        {
            return currentTab;
        }

        public VisualElement VisualElement()
        {
            return root;
        }
    }
}