using System.Collections.Generic;

public class MigrationService
{
    private readonly Version latestVersion;
    private readonly List<Migration> migrations = new List<Migration>();

    public MigrationService()
    {
        migrations.Add(new GlobalToLocalMigration());
        latestVersion = new Version(0, 1, 0);
    }

    public LandDetails Migrate(LandDetails details)
    {
        var version = new Version(details.v);

        while (!latestVersion.Equals(latestVersion))
        {
            foreach (var m in migrations)
            {
                if (m.Accepts(version))
                {
                    details = m.Migrate(details);
                    version = m.GetTarget();
                }
            }
        }

        return details;
    }

    public string GetLatestVersion()
    {
        return latestVersion.ToString();
    }
}
