using UnityEngine.UIElements;

namespace Source.Ui.Menu
{
    public class Help : UxmlElement
    {
        public Help():base("Ui/Menu/Help",true)
        {
            var content = this.Q<ScrollView>("content");
            Utils.Utils.IncreaseScrollSpeed(content, 600);
            var leftBar = this.Q<ScrollView>("leftBar");
            Utils.Utils.IncreaseScrollSpeed(leftBar, 600);
            var basicShortcuts = Utils.Utils.Create("Ui/Menu/HelpBasicShortcuts");
            content.Add(basicShortcuts);
            var basicShortcutsButton = new Button
            {
                text = "Basic shortcuts"
            };
            basicShortcutsButton.clickable.clicked += () => content.ScrollTo(basicShortcuts);
            basicShortcutsButton.AddToClassList("utopia-button-primary");
            basicShortcutsButton.AddToClassList("left-align-text");
            leftBar.Add(basicShortcutsButton);
        }
    }
}