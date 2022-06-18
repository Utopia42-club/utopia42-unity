using System.Collections.Generic;
using Source.Model;

namespace Source.Service.Migration
{
    public class MigrationService
    {
        private readonly Version latestVersion;
        private readonly List<Migration> migrations = new();

        public MigrationService()
        {
            migrations.Add(new GlobalToLocalMigration());
            migrations.Add(new RemoveRegionMigration());
            migrations.Add(new MetaDetachMigration());
            latestVersion = new Version(0, 3, 0);
        }

        public LandDetails Migrate(Land land, LandDetails details)
        {
            var version = new Version(details.v);
            while (!version.Equals(latestVersion))
            {
                foreach (var m in migrations)
                {
                    if (m.Accepts(version))
                    {
                        details = m.Migrate(land, details);
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
}