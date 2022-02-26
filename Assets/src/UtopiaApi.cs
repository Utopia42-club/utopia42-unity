using System;
using Newtonsoft.Json;
using src;
using src.Model;
using src.Service;
using UnityEngine;

public class UtopiaApi : MonoBehaviour
{
    public string PlaceBlock(String request)
    {
        var req = JsonConvert.DeserializeObject<PlaceBlockRequest>(request);
        var placed = Player.INSTANCE.ApiPutBlock(new Vector3(req.position.x, req.position.y, req.position.z),
            WorldService.INSTANCE.GetBlockType(req.type));
        return JsonConvert.SerializeObject(placed);
    }

    public string GetPlayerPosition()
    {
        var pos = Player.INSTANCE.transform.position;
        return JsonConvert.SerializeObject(new SerializableVector3(pos));
    }

    public string GetMarkers()
    {
        return JsonConvert.SerializeObject(WorldService.INSTANCE.GetMarkers());
    }

    public string GetPlayerLands(string walletId)
    {
        return JsonConvert.SerializeObject(WorldService.INSTANCE.GetLandsFor(walletId));
    }

    public string GetCurrentLand()
    {
        return JsonConvert.SerializeObject(WorldService.INSTANCE.GetLandByPosition(Player.INSTANCE.transform.position));
    }

    public string GetBlockTypes()
    {
        return JsonConvert.SerializeObject(WorldService.INSTANCE.GetNonMetaBlockTypes());
    }

    private class PlaceBlockRequest
    {
        public string type;
        public SerializableVector3 position;
    }
}