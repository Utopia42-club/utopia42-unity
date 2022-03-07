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
using Object = System.Object;

public class UtopiaApi : MonoBehaviour
{
    public UnityEvent<object> CurrentLand()
    {
        return FindObjectOfType<Owner>().currentLandChanged;
    }

    public bool PlaceBlock(String request)
    {
        var req = JsonConvert.DeserializeObject<PlaceBlockRequest>(request);
        var placed = Player.INSTANCE.ApiPutBlock(new VoxelPosition(req.position),
            WorldService.INSTANCE.GetBlockType(req.type));
        return placed;
    }

    public Dictionary<Vector3Int, bool> PlaceMetaBlocks(String request)
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
            if ((type == null || type.name.Equals("air")) && metaType != null && !WorldService.INSTANCE.IsSolid(pos))
                type = WorldService.INSTANCE.GetBlockType("grass");
            if (type != null) blocks.Add(pos, type);

            if (metaType == null) continue;
            var propsString = req.type.metaBlock.properties;
            var props = metaType.DeserializeProps(propsString == null || propsString.Equals("")
                ? "{}"
                : req.type.metaBlock.properties);
            metaBlocks.Add(pos, new Tuple<MetaBlockType, object>(metaType, props));
        }

        var baseBlocksResult = Player.INSTANCE.ApiPutBlocks(blocks);
        var metaBlocksResult = Player.INSTANCE.ApiPutMetaBlocks(metaBlocks);

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
        return WorldService.INSTANCE.GetLandsFor(walletId);
    }

    public Land GetCurrentLand()
    {
        return WorldService.INSTANCE.GetLandByPosition(Player.INSTANCE.transform.position);
    }

    public List<string> GetBlockTypes()
    {
        return WorldService.INSTANCE.GetNonMetaBlockTypes();
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