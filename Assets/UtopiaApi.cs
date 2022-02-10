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
        player.PutBlock(new Vector3(req.x, req.y, req.z), VoxelService.INSTANCE.GetBlockType(req.type));
    }

    public string GetPlayerPosition()
    {
        var pos = Player.INSTANCE.transform.position;
        return JsonConvert.SerializeObject(new Position(pos.x, pos.y, pos.z));
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
    
    public class PlaceBlockRequest
    {
        public string type;
        public int x;
        public int y;
        public int z;
    }
}