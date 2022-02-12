using System;
using Newtonsoft.Json;
using src;
using src.Service;
using UnityEngine;

public class UtopiaApi : MonoBehaviour
{
    public Player player;

    public string PlaceBlock(String request)
    {
        var req = JsonConvert.DeserializeObject<PlaceBlockRequest>(request);
        var placed = player.PutBlock(new Vector3(req.position.x, req.position.y, req.position.z),
            VoxelService.INSTANCE.GetBlockType(req.type), true);
        return JsonConvert.SerializeObject(placed);
    }

    public string GetPlayerPosition()
    {
        var pos = Player.INSTANCE.transform.position;
        return JsonConvert.SerializeObject(new Position(pos.x, pos.y, pos.z));
    }

    public string GetMarkers()
    {
        var markers = new Marker[]
        {
            new Marker("Marker 1", new Position(1, 2, 3)),
            new Marker("Marker 2", new Position(2, 3, 4)),
            new Marker("Marker 3", new Position(3, 4, 5)),
        };
        return JsonConvert.SerializeObject(markers);
    }

    public string GetPlayerLands(string walletId)
    {
        return JsonConvert.SerializeObject(VoxelService.INSTANCE.GetLandsFor(walletId));
    }

    public string GetBlockTypes()
    {
        return JsonConvert.SerializeObject(VoxelService.INSTANCE.GetBlockTypes());
    }

    private class Position
    {
        public float x;
        public float y;
        public float z;

        public Position(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    private class PlaceBlockRequest
    {
        public string type;
        public Position position;
    }

    private class Marker
    {
        public string name;
        public Position position;

        public Marker(string name, Position position)
        {
            this.name = name;
            this.position = position;
        }
    }
}