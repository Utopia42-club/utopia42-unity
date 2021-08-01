using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    public Dictionary<byte, Sprite> blockIcons = new Dictionary<byte, Sprite>();
    public Sprite[] sprites;

    void Start()
    {
        foreach (var sp in sprites)
        {
            var id = VoxelService.INSTANCE.GetBlockType(sp.name).id;
            blockIcons[id] = sp;
        }
    }

}
