internal class GlobalToLocalMigration : Migration
{
    public GlobalToLocalMigration()
        : base(new ApplicationVersion[] { new ApplicationVersion(0, 0, 0), new ApplicationVersion(0, 0, 1) },
            new ApplicationVersion(0, 1, 0))
    {
    }

    public override LandDetails Migrate(LandDetails details)
    {
        //var x1 = details.region.x1;
        //var y1 = details.region.y1;

        //foreach(var change in details.changes)
        //{
        //    change.
        //}
        return null;
    }
}
