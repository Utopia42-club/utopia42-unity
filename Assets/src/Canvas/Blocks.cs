using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    public static Dictionary<byte, Sprite> blockIcons = new Dictionary<byte, Sprite>();
    public Sprite[] sprites;

    void Start()
    {
        if(blockIcons == null)
        {
            foreach (var sp in sprites)
            {
                var id = VoxelService.INSTANCE.GetBlockType(sp.name).id;
                blockIcons[id] = sp;
            }
        }
    }

}
