using System;
using Newtonsoft.Json;
using src;
using src.Service;
using UnityEngine;

public class UtopiaApi : MonoBehaviour
{
    public Player player;

    public void PlaceBlock(String request)
    {
        var req = JsonConvert.DeserializeObject<PlaceBlockRequest>(request);
        player.PutBlock(new Vector3(req.x, req.y, req.z), VoxelService.INSTANCE.GetBlockType(req.type), true);
    }

    public class PlaceBlockRequest
    {
        public string type;
        public int x;
        public int y;
        public int z;
    }
}