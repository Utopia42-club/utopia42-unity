using System.Collections.Generic;

internal abstract class Migration
{
    private readonly HashSet<ApplicationVersion> sourceVersions;
    private readonly ApplicationVersion targetVersion;

    protected Migration(ApplicationVersion[] sourceVersions, ApplicationVersion targetVersion)
    {
        this.sourceVersions = new HashSet<ApplicationVersion>(sourceVersions);
        this.targetVersion = targetVersion;
    }

    public bool Accepts(ApplicationVersion version)
    {
        return sourceVersions.Contains(version);
    }

    public ApplicationVersion GetTarget()
    {
        return targetVersion;
    }

    public abstract LandDetails Migrate(LandDetails details);
}