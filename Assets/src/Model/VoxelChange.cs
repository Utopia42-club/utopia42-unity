[System.Serializable]
public class VoxelChange
{
    public int[] voxel;
    public string name;

    public override string ToString()
    {
        return name;
        //return string.Format("({3}", voxel[0], voxel[1], voxel[2], name);
    }
}
