namespace Source.Ui.Loading
{
    public class LoadingController
    {
        private readonly int id;

        public LoadingController(int id)
        {
            this.id = id;
        }

        public void Close()
        {
            LoadingLayer.Hide(id);
        }
    }
}