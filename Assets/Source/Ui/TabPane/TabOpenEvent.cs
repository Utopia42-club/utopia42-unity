namespace Source.Ui.TabPane
{
    public class TabOpenEvent
    {
        public readonly TabPane TabPane;

        public TabOpenEvent(TabPane tabPane)
        {
            this.TabPane = tabPane;
        }
    }
}