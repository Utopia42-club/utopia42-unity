using System.Collections.Generic;

[System.Serializable]
public class LandDetails
{
    public string v;
    public string wallet;
    public Land region;
    public Dictionary<string, VoxelChange> changes;
}
