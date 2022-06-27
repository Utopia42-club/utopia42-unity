namespace Source.Ui.Snack
{
    public class SnackController
    {
        private readonly Snack snack;
        private readonly int id;

        public SnackController(Snack snack, int id)
        {
            this.snack = snack;
            this.id = id;
        }

        public void Close()
        {
            SnackService.INSTANCE.Close(id);
        }

        public Snack Snack => snack;
    }
}