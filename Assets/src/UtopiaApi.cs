using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.MarkerBlock;
using src.Model;
using src.Service;
using UnityEngine;
using UnityEngine.Events;

public class UtopiaApi : MonoBehaviour
{
    public UnityEvent<object> CurrentLand()
    {
        return FindObjectOfType<Owner>().currentLandChanged;
    }
    
    public UnityEvent<object> BlockPlaced()
    {
        return WorldService.INSTANCE.blockPlaced;
    }
    
    public Dictionary<Vector3Int, bool> PlaceMetaBlocks(string request)
    {
        var reqs = JsonConvert.DeserializeObject<List<PlaceMetaBlockRequest>>(request);

        var blocks = new Dictionary<VoxelPosition, BlockType>();
        var metaBlocks = new Dictionary<VoxelPosition, Tuple<MetaBlockType, object>>();
        var placed = new Dictionary<Vector3Int, bool>();

        foreach (var req in reqs)
        {
            var pos = new VoxelPosition(req.position);
            var globalPos = pos.ToWorld();
            if (placed.ContainsKey(globalPos))
            {
                Debug.Log("Duplicate position detected");
                continue;
            }
            placed.Add(globalPos, false);

            var metaType = (MetaBlockType) (req.type.metaBlock?.type == null
                ? null
                : WorldService.INSTANCE.GetBlockType(req.type.metaBlock.type, false, true));

            var type = req.type.blockType == null ? null : WorldService.INSTANCE.GetBlockType(req.type.blockType, true);
            //FIXME what if data was not loaded?
            if ((type == null || type.name.Equals("air")) && metaType != null && !WorldService.INSTANCE.IsSolidIfLoaded(pos))
                type = WorldService.INSTANCE.GetBlockType("grass");
            if (type != null) blocks.Add(pos, type);

            if (metaType == null) continue;
            var propsString = req.type.metaBlock.properties;
            var props = metaType.DeserializeProps(propsString == null || propsString.Equals("")
                ? "{}"
                : req.type.metaBlock.properties);
            metaBlocks.Add(pos, new Tuple<MetaBlockType, object>(metaType, props));
        }

        var baseBlocksResult = PutBlocks(blocks);
        var metaBlocksResult = PutMetaBlocks(metaBlocks);

        foreach (var pos in placed.Keys.ToArray())
        {
            if ((baseBlocksResult.TryGetValue(pos, out var baseResult) && baseResult) ||
                metaBlocksResult.TryGetValue(pos, out var metaResult) && metaResult)
                placed[pos] = true;
        }

        return placed;
    }

    public SerializableVector3 GetPlayerPosition()
    {
        var pos = Player.INSTANCE.transform.position;
        return new SerializableVector3(pos);
    }

    public List<Marker> GetMarkers()
    {
        return WorldService.INSTANCE.GetMarkers();
    }

    public List<Land> GetPlayerLands(string walletId)
    {
        return WorldService.INSTANCE.GetPlayerLands();
    }

    public Land GetCurrentLand()
    {
        return WorldService.INSTANCE.GetLandByPosition(Player.INSTANCE.transform.position);
    }

    public List<string> GetBlockTypes()
    {
        return WorldService.INSTANCE.GetNonMetaBlockTypes();
    }

    private static bool PutBlock(VoxelPosition vp, BlockType getBlockType)
    {
        return PutBlocks(new Dictionary<VoxelPosition, BlockType> {{vp, getBlockType}})[vp.ToWorld()];
    }

    public static Dictionary<Vector3Int, bool> PutBlocks(Dictionary<VoxelPosition, BlockType> blocks)
    {
        var result = new Dictionary<Vector3Int, bool>();
        var toBePut = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
        foreach (var vp in blocks.Keys)
        {
            var type = blocks[vp];
            var blockPos = vp.ToWorld();
            if (!(type is MetaBlockType) && Player.INSTANCE.CanEdit(blockPos, out var ownerLand))
            {
                toBePut.Add(vp, new Tuple<BlockType, Land>(type, ownerLand));
                result.Add(blockPos, true);
                continue;
            }
            result.Add(blockPos, false);
        }

        World.INSTANCE.PutBlocks(toBePut);
        return result;
    }

    public static Dictionary<Vector3Int, bool> PutMetaBlocks(
        Dictionary<VoxelPosition, Tuple<MetaBlockType, object>> metaBlocks)
    {
        var result = new Dictionary<Vector3Int, bool>();
        foreach (var vp in metaBlocks.Keys)
        {
            var (type, props) = metaBlocks[vp];
            result.Add(vp.ToWorld(), World.INSTANCE.PutMetaWithProps(vp, type, props));
        }

        return result;
    }
    
    public static UtopiaApi INSTANCE => GameObject.Find("UtopiaApi").GetComponent<UtopiaApi>();

    private class PlaceBlockRequest
    {
        public string type;
        public SerializableVector3 position;
    }

    private class PlaceMetaBlockRequest
    {
        public MetaBlockTypeData type;
        public SerializableVector3 position;
    }

    [Serializable]
    private class MetaBlockTypeData
    {
        public string blockType;
        public MetaBlockData metaBlock;
    }

    [Serializable]
    private class MetaBlockData
    {
        public string type;
        public string properties;
    }
}