using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Source.Ui.TabPane
{
    public class TabPane : UxmlElement
    {
        private readonly TemplateContainer root;
        private readonly List<TabConfiguration> tabConfigs;
        private readonly List<Button> tabButtons = new();
        private int currentTab;
        private readonly VisualElement tabBody;
        private readonly VisualElement tabButtonsArea;
        private readonly VisualElement leftActions;
        private readonly VisualElement rightActions;

        public TabPane(List<TabConfiguration> tabConfigs) : base("Ui/TebPane/TabPane", true)
        {
            this.tabConfigs = tabConfigs;
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

        public void OpenTab(int index)
        {
            var config = tabConfigs[index];
            var tabBodyContent = config.VisualElement;
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

        public void SetTabButtonsAreaVisibility(bool visible)
        {
            tabButtonsArea.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        public void AddLeftAction(VisualElement visualElement)
        {
            leftActions.Add(visualElement);
        }

        public void AddRightAction(VisualElement visualElement)
        {
            rightActions.Insert(0, visualElement);
        }
    }
}