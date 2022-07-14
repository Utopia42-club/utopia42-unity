using System.Collections.Generic;
using Source.Model;

namespace Source.Service.Migration
{
    internal abstract class Migration
    {
        private readonly HashSet<Version> sourceVersions;
        private readonly Version targetVersion;

        protected Migration(Version[] sourceVersions, Version targetVersion)
        {
            this.sourceVersions = new HashSet<Version>(sourceVersions);
            this.targetVersion = targetVersion;
        }

        public bool Accepts(Version version)
        {
            return sourceVersions.Contains(version);
        }

        public Version GetTarget()
        {
            return targetVersion;
        }

        public abstract LandDetails Migrate(Land land, LandDetails details);
    }
}