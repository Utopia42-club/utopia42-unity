using System.Collections.Generic;
using src.Model;

namespace src.Service.Migration
{
    public class MigrationService
    {
        private readonly Version latestVersion;
        private readonly List<global::src.Service.Migration.Migration> migrations = new List<global::src.Service.Migration.Migration>();

        public MigrationService()
        {
            migrations.Add(new GlobalToLocalMigration());
            migrations.Add( new RemoveRegionMigration());
            latestVersion = new Version(0, 2, 0);
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
