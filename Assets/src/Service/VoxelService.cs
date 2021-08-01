using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VoxelService
{
    public static VoxelService INSTANCE = new VoxelService();
    private List<Land> lands;
    private Dictionary<byte, BlockType> types = new Dictionary<byte, BlockType>();
    private Dictionary<Vector3Int, Dictionary<Vector3Int, byte>> changes = null;
    private Dictionary<string, List<Land>> ownersLands;

    public VoxelService()
    {
        types[0] = new BlockType(0, "air", false, 0, 0, 0, 0, 0, 0);
        types[1] = new BlockType(1, "grass", true, 10, 10, 10, 10, 7, 11) ;
        types[2] = new BlockType(2, "bedrock", true, 0, 0, 0, 0, 0, 0);
        types[3] = new BlockType(3, "dirt", true, 7, 7, 7, 7, 7, 7);
        types[4] = new BlockType(4, "stone", true, 29, 29, 29, 29, 29, 29);
        types[5] = new BlockType(5, "sand", true, 27, 27, 27, 27, 27, 27);
        types[6] = new BlockType(6, "bricks", true, 3, 3, 3, 3, 3, 3);
        types[7] = new BlockType(7, "wood", true, 19, 19, 19, 19, 20, 20);
        types[8] = new BlockType(8, "planks", true, 21, 21, 21, 21, 21, 21);
        types[9] = new BlockType(9, "cobblestone", true, 4, 4, 4, 4, 4, 4);
        types[10] = new BlockType(10, "black_terracotta", true, 1, 1, 1, 1, 1, 1);
        types[11] = new BlockType(11, "blue_wool", true, 2, 2, 2, 2, 2, 2);
        types[12] = new BlockType(12, "cyan_wool", true, 5, 5, 5, 5, 5, 5);
        types[13] = new BlockType(13, "diamond", true, 6, 6, 6, 6, 6, 6);
        types[14] = new BlockType(14, "end_stone", true, 8, 8, 8, 8, 8, 8);
        types[15] = new BlockType(15, "gold", true, 9, 9, 9, 9, 9, 9);
        types[16] = new BlockType(16, "gravel", true, 12, 12, 12, 12, 12, 12);
        types[17] = new BlockType(17, "green_wool", true, 13, 13, 13, 13, 13, 13);
        types[18] = new BlockType(18, "ice", true, 14, 14, 14, 14, 14, 14);
        types[19] = new BlockType(19, "lime_wool", true, 15, 15, 15, 15, 15, 15);
        types[20] = new BlockType(20, "magma", true, 16, 16, 16, 16, 16, 16);
        types[21] = new BlockType(21, "mossy_stone_bricks", true, 17, 17, 17, 17, 17, 17);
        types[22] = new BlockType(22, "nether_bricks", true, 18, 18, 18, 18, 18, 18);
        types[23] = new BlockType(23, "polished_andesite", true, 22, 22, 22, 22, 22, 22);
        types[24] = new BlockType(24, "purple_wool", true, 23, 23, 23, 23, 23, 23);
        types[25] = new BlockType(25, "purpur", true, 24, 24, 24, 24, 24, 24);
        types[26] = new BlockType(26, "quartz", true, 25, 25, 25, 25, 25, 25);
        types[27] = new BlockType(27, "red_wool", true, 26, 26, 26, 26, 26, 26);
        types[28] = new BlockType(28, "snow", true, 28, 28, 28, 28, 28, 28);
        types[29] = new BlockType(29, "stone_bricks", true, 30, 30, 30, 30, 30, 30);
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
        byte bedrock = GetBlockType("bedrock").id;
        byte body;
        byte top;
        if (position.y == 0)
        {
            InitGroundLevel(position, voxels, bedrock);
            return;
        }
        if (position.y < 0)
        {
            top = body = bedrock;
        }
        else
            body = top = GetBlockType("air").id;

        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(2); z++)
            {
                FillAtXY(top, body, x, z, voxels);
            }
        }
    }

    private void InitGroundLevel(Vector3Int position, byte[,,] voxels, byte bedrock)
    {
        byte body, top;
        byte grass = GetBlockType("grass").id;
        byte dirt = GetBlockType("dirt").id;
        var player = Player.INSTANCE;
        for (var x = 0; x < Chunk.CHUNK_WIDTH; ++x)
        {
            for (var z = 0; z < Chunk.CHUNK_WIDTH; ++z)
            {
                var owns = player.Owns(
                    new Vector3Int(x + position.x * Chunk.CHUNK_WIDTH, 0, z + position.z * Chunk.CHUNK_WIDTH)
                );
                top = body = bedrock;
                if (owns)
                {
                    top = grass;
                    body = dirt;
                }
                FillAtXY(top, body, x, z, voxels);
            }
        }
    }

    private void FillAtXY(byte top, byte body, int x, int z, byte[,,] voxels)
    {
        int maxy = voxels.GetLength(1) - 1;

        voxels[x, maxy, z] = top;
        for (int y = 0; y < maxy; y++)
            voxels[x, y, z] = body;
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

    public int GetBlockTypesCount()
    {
        return types.Values.Count;
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

    public List<Land> getLandsFor(string walletId)
    {
        List<Land> res = null;
        ownersLands.TryGetValue(walletId, out res);
        return res;
    }

    public IEnumerator Initialize(Loading loading, Action onDone)
    {
        if (IsInitialized()) yield break;
        var changes = new Dictionary<Vector3Int, Dictionary<Vector3Int, byte>>();
        yield return LoadDetails(loading, land =>
        {
            foreach (var entry in land.changes)
            {
                var change = entry.Value;
                var coords = entry.Key.Split('_');
                var position = new VoxelPosition(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));

                var type = GetBlockType(change.name);
                if (type == null) continue;

                Dictionary<Vector3Int, byte> chunk;
                if (!changes.TryGetValue(position.chunk, out chunk))
                    changes[position.chunk] = chunk = new Dictionary<Vector3Int, byte>();
                chunk[position.local] = type.id;
            }
        });

        this.changes = changes;
        onDone.Invoke();
        yield break;
    }

    private IEnumerator LoadDetails(Loading loading, Action<LandDetails> consumer)
    {
        loading.UpdateText("Loading Wallets And Lands...");
        var ownersLands = new Dictionary<string, List<Land>>();
        yield return EthereumClientService.INSTANCE.getLands(ownersLands);
        this.ownersLands = ownersLands;

        var lands = new List<Land>();
        foreach (var val in ownersLands.Values)
            lands.AddRange(val);

        var landsDetails = new LandDetails[lands.Count];
        var enums = new IEnumerator[lands.Count];

        for (int i = 0; i < lands.Count; i++)
        {
            var land = lands[i];
            var idx = i;
            if (!string.IsNullOrWhiteSpace(land.ipfsKey))
                enums[i] = IpfsClient.INSATANCE.GetLandDetails(land.ipfsKey, consumer);
        }

        for (int i = 0; i < lands.Count; i++)
            if (enums[i] != null)
            {
                loading.UpdateText(string.Format("Loading Changes ({0}/{1})...", i, enums.Length));
                yield return enums[i];
            }
    }

    public List<LandDetails> GetLandsChanges(string wallet, List<Land> lands)
    {
        var regionChanges = new List<LandDetails>();
        foreach (var l in lands)
        {
            var ld = new LandDetails();
            ld.changes = new Dictionary<string, VoxelChange>();
            ld.region = l;
            ld.v = "0.0.1";
            ld.wallet = wallet;
            regionChanges.Add(ld);
        }


        foreach (var chunkEntry in changes)
        {
            var cpos = chunkEntry.Key;
            var chunkChanges = chunkEntry.Value;
            foreach (var voxelEntry in chunkChanges)
            {
                var vpos = voxelEntry.Key;
                var worldPos = VoxelPosition.ToWorld(cpos, vpos);
                for (int landIdx = 0; landIdx < lands.Count; landIdx++)
                {
                    var land = lands[landIdx];
                    if (land.x1 <= worldPos.x && worldPos.x <= land.x2 && land.y1 <= worldPos.z && worldPos.z <= land.y2)
                    {
                        var key = string.Format("{0}_{1}_{2}", worldPos.x, worldPos.y, worldPos.z);
                        var change = new VoxelChange();
                        change.name = VoxelService.INSTANCE.GetBlockType(voxelEntry.Value).name;
                        change.voxel = new int[] { worldPos.x, worldPos.y, worldPos.z };
                        regionChanges[landIdx].changes[key] = change;
                        break;
                    }
                }
            }
        }
        return regionChanges;
    }

    public void AddChange(VoxelPosition pos, byte id)
    {
        Dictionary<Vector3Int, byte> vc;
        if (!changes.TryGetValue(pos.chunk, out vc))
        {
            vc = new Dictionary<Vector3Int, byte>();
            changes[pos.chunk] = vc;
        }
        vc[pos.local] = id;
    }

    public Dictionary<string, List<Land>> GetOwnersLands()
    {
        return ownersLands;
    }

    public bool IsInitialized()
    {
        return this.changes != null;
    }

    public IEnumerator ReloadLandsFor(string wallet)
    {
        yield return EthereumClientService.INSTANCE.getLandsForOwner(wallet, lands => ownersLands[wallet] = lands);
    }
}
