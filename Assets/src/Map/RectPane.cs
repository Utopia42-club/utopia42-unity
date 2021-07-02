using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RectPane : MonoBehaviour
{
    [SerializeField]
    private RectTransform playerPosIndicator;
    private readonly List<GameObject> landIndicators = new List<GameObject>();

    void Start()
    {
        var manager = GameManager.INSTANCE;
        if (manager.GetSTate() == GameManager.State.MAP) Init();

        manager.stateChange.AddListener(s =>
        {
            if (s == GameManager.State.MAP) Init();
            else DestroyRects();
        });
    }

    void Update()
    {

    }

    private void Init()
    {
        VoxelService service = VoxelService.INSTANCE;
        if (!service.IsInitialized()) return;

        var playerPos = Player.INSTANCE.transform.position;
        playerPosIndicator.localPosition = new Vector3(playerPos.x, playerPos.z, 0);
        var transform = GetComponent<RectTransform>();
        transform.anchoredPosition = new Vector3(-playerPos.x, -playerPos.z, 0);

        foreach (var entry in service.GetOwnersLands())
        {
            bool owned = entry.Key.Equals(Settings.WalletId());
            foreach (var land in entry.Value)
                Add(land, owned);
        }
    }

    private void DestroyRects()
    {
        foreach (var lo in landIndicators)
            Destroy(lo);
        landIndicators.Clear();
    }

    private void Add(Land land, bool owned)
    {
        var landObject = new GameObject();
        var transform = landObject.AddComponent<RectTransform>();

        transform.SetParent(this.transform);
        transform.SetAsFirstSibling();
        transform.pivot = new Vector2(0, 0);
        transform.localPosition = new Vector3(land.x1, land.y1, 0);
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, land.y2 - land.y1);
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, land.x2 - land.x1);
        landObject.AddComponent<CanvasRenderer>();
        landObject.AddComponent<Image>().color = owned ? Color.green : Color.gray;
    }
}
