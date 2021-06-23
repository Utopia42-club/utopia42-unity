using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    private Dictionary<byte, Sprite> blockIcons = new Dictionary<byte, Sprite>();
    private World world;
    private int selectedSlot = 0;

    public Player player;
    public RectTransform highlight;
    public ItemSlot[] slots;
    public Sprite[] sprites;


    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        foreach (var sp in sprites)
        {
            var id = VoxelService.INSTANCE.GetBlockType(sp.name).id;
            blockIcons[id] = sp;
        }

        foreach (var slot in slots)
        {
            var id = slot.itemId;
            slot.icon.sprite = blockIcons[id];
            slot.icon.enabled = true;
        }

        SelectedChanged();
    }

    private void Update()
    {
        bool dec = Input.GetKeyDown(KeyCode.Q);
        bool inc = Input.GetKeyDown(KeyCode.E);
        if (dec || inc)
        {
            if (dec) selectedSlot--;
            if (inc) selectedSlot++;
            selectedSlot = (selectedSlot + slots.Length) % slots.Length;
            SelectedChanged();
        }
    }

    private void SelectedChanged()
    {
        highlight.position = slots[selectedSlot].icon.transform.position;
        player.selectedBlockId = slots[selectedSlot].itemId;
    }

}

[System.Serializable]
public class ItemSlot
{
    public byte itemId;
    public Image icon;
}