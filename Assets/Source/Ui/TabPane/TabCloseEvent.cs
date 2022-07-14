namespace Source.Ui.TabPane
{
    public class TabCloseEvent
    {
        public readonly TabPane TabPane;

        public TabCloseEvent(TabPane tabPane)
        {
            TabPane = tabPane;
        }
    }
}