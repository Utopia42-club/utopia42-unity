using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src;
using src.Canvas;
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

    public bool PlaceBlock(String request)
    {
        var req = JsonConvert.DeserializeObject<PlaceBlockRequest>(request);
        var placed = Player.INSTANCE.ApiPutBlock(new Vector3(req.position.x, req.position.y, req.position.z),
            WorldService.INSTANCE.GetBlockType(req.type));
        return placed;
    }

    public Dictionary<Vector3, bool> PlaceBlocks(String request)
    {
        var reqs = JsonConvert.DeserializeObject<List<PlaceBlockRequest>>(request);
        var placed = Player.INSTANCE.ApiPutBlocks(reqs.ToDictionary(
            req => new Vector3(req.position.x, req.position.y, req.position.z),
            req => WorldService.INSTANCE.GetBlockType(req.type)));
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

    private class PlaceBlockRequest
    {
        public string type;
        public SerializableVector3 position;
    }
}