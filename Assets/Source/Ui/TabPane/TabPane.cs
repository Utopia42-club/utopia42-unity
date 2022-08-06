using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Source.Ui.TabPane
{
    public class TabPane : UxmlElement
    {
        public event Action<TabConfiguration, VisualElement> TabClosed = (t, v) => { };
        public event Action<TabConfiguration, VisualElement> TabOpened = (t, v) => { };
        private readonly TemplateContainer root;
        private readonly List<TabConfiguration> tabConfigs;
        private readonly bool useCache;
        private readonly List<Button> tabButtons = new();
        private int currentTab = -1;
        private readonly VisualElement tabBody;
        private readonly VisualElement tabButtonsArea;
        private readonly VisualElement leftActions;
        private readonly VisualElement rightActions;
        private readonly Dictionary<int, VisualElement> tabBodiesCache = new();

        public TabPane(List<TabConfiguration> tabConfigs, bool useCache = true)
            : base(typeof(TabPane), true)
        {
            this.tabConfigs = tabConfigs;
            this.useCache = useCache;
            tabBody = this.Q<VisualElement>("tabBody");
            tabButtonsArea = this.Q<VisualElement>("tabs");
            leftActions = this.Q<VisualElement>("leftActions");
            rightActions = this.Q<VisualElement>("rightActions");

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

        public void OpenTab(int index, bool reloadOnSameTab = false)
        {
            if (currentTab == index && !reloadOnSameTab) return;
            var config = tabConfigs[index];
            if (!tabBodiesCache.TryGetValue(index, out var tabBodyContent))
            {
                tabBodyContent = config.visualElementFactory?.Invoke() ?? config.VisualElement;
                tabBodyContent.style.paddingRight = tabBodyContent.style.paddingLeft = 5;
                if (useCache)
                    tabBodiesCache[index] = tabBodyContent;
            }

            CloseCurrent();
            tabBody.Add(tabBodyContent);
            foreach (var button in tabButtons)
                button.RemoveFromClassList("selected-tab");
            tabButtons[index].AddToClassList("selected-tab");
            currentTab = index;
            var e = new TabOpenEvent(this);
            tabConfigs[currentTab].onTabOpen?.Invoke(e);
            if (tabBodyContent is TabOpenListener listener)
                listener.OnTabOpen(e);
            TabOpened.Invoke(config, tabBodyContent);
        }

        public VisualElement GetTabBody()
        {
            return tabBody;
        }

        public int GetCurrentTabIndex()
        {
            return currentTab;
        }

        public void AddLeftAction(VisualElement visualElement)
        {
            leftActions.Add(visualElement);
        }

        public void AddRightAction(VisualElement visualElement)
        {
            rightActions.Insert(0, visualElement);
        }

        public void CloseCurrent()
        {
            if (currentTab != -1)
            {
                var e = new TabCloseEvent(this);
                tabConfigs[currentTab].onTabClose?.Invoke(e);
                var currentTabContent = GetCurrentTabContent();
                if (currentTabContent is TabCloseListener listener)
                    listener.OnTabClose(e);
                TabClosed.Invoke(tabConfigs[currentTab], currentTabContent);
            }

            tabBody.Clear();
        }

        public VisualElement GetCurrentTabContent()
        {
            return tabBody.Children().ElementAtOrDefault(0);
        }

        public void ReloadTab()
        {
            OpenTab(currentTab, true);
        }
    }
}