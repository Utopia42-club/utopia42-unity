using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VoxelService
{
    private byte air = 0;
    private byte grass = 1;
    private byte bedrock = 2;
    private byte dirt = 3;
    private List<Land> lands;
    private Dictionary<byte, BlockType> types = new Dictionary<byte, BlockType>();
    private Dictionary<Vector3Int, Dictionary<Vector3Int, byte>> changes = null;

    public VoxelService()
    {
        types[0] = new BlockType(0, "air", false, 0, 0, 0, 0, 0, 0);
        types[1] = new BlockType(1, "grass", true, 0, 0, 0, 0, 0, 0);
        types[2] = new BlockType(2, "bedrock", true, 0, 0, 0, 0, 0, 0);
        types[3] = new BlockType(3, "dirt", true, 0, 0, 0, 0, 0, 0);
    }

    public void FillChunk(Vector3Int coordinate, byte[,,] voxels)
    {
        InitiateChunk(coordinate, voxels);

        Dictionary<Vector3Int, byte> chunkChanges;
        if (changes.TryGetValue(coordinate, out chunkChanges))
        {
            foreach (var change in chunkChanges)
            {
                var voxel = change.Key;
                voxels[voxel.x, voxel.y, voxel.z] = change.Value;
            }
        }
    }


    private void InitiateChunk(Vector3Int position, byte[,,] voxels)
    {
        byte body;
        byte top;
        if (position.y < 0)
        {
            top = body = bedrock;
        }
        else if (position.y == 0)
        {
            body = dirt;
            top = grass;
        }
        else
            body = top = air;

        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(2); z++)
            {
                int maxy = voxels.GetLength(1) - 1;

                voxels[x, maxy, z] = top;
                for (int y = 0; y < maxy; y++)
                    voxels[x, y, z] = body;
            }
        }

    }

    public BlockType GetBlockType(byte id)
    {
        return types[id];
    }

    public BlockType GetBlockType(string name)
    {
        foreach (var entry in types)
        {
            if (entry.Value.name.Equals(name))
                return entry.Value;
        }
        Debug.LogError("Invalid block type: " + name);
        return null;
    }

    public bool IsSolid(VoxelPosition vp)
    {
        Dictionary<Vector3Int, byte> chunkChanges;
        if (changes.TryGetValue(vp.chunk, out chunkChanges))
        {
            byte type;
            if (chunkChanges.TryGetValue(vp.local, out type))
            {
                return GetBlockType(type).isSolid;
            }
        }

        return vp.chunk.y <= 0;
    }

    public IEnumerator Initialize()
    {
        if (IsInitialized()) yield break;

        //List<Land> lands = new List<Land>();
        //yield return EthereumClientService.INSTANCE.getLands(l => lands.AddRange(l));

        //var details = new List<LandDetails>();
        //foreach (var land in lands)
        //    if (!string.IsNullOrWhiteSpace(land.ipfsKey))
        //        yield return IpfsClient.INSATANCE.GetLandDetails(land.ipfsKey, l => details.Add(l));

        var changes = new Dictionary<Vector3Int, Dictionary<Vector3Int, byte>>();
        //foreach (var land in details)
        //{
        //    foreach (var entry in land.changes)
        //    {
        //        var change = entry.Value;
        //        var position = new VoxelPosition(change.voxel[0], change.voxel[1], change.voxel[2]);

        //        var type = GetBlockType(change.name);
        //        if (type == null) continue;

        //        Dictionary<Vector3Int, byte> chunk;
        //        if (!changes.TryGetValue(position.chunk, out chunk))
        //            changes[position.chunk] = chunk = new Dictionary<Vector3Int, byte>();
        //        chunk[position.local] = type.id;
        //    }
        //}
        this.changes = changes;
        yield break;
    }

    public bool IsInitialized()
    {
        return this.changes != null;
    }
}
