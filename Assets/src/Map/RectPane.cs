using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RectPane : MonoBehaviour
{
    [SerializeField]
    private RectTransform playerPosIndicator;
    private readonly HashSet<GameObject> landIndicators = new HashSet<GameObject>();
    private readonly HashSet<GameObject> drawnLandIndicators = new HashSet<GameObject>();

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
            Color c = entry.Key.Equals(Settings.WalletId()) ? Color.green : Color.gray;
            foreach (var land in entry.Value)
                Add(land.x1, land.x2, land.y1, land.y2, c);
        }
    }

    private void DestroyRects()
    {
        foreach (var lo in landIndicators)
            Destroy(lo);
        landIndicators.Clear();
    }

    private GameObject Add(long x1, long x2, long y1, long y2, Color color)
    {
        var landObject = new GameObject();
        var transform = landObject.AddComponent<RectTransform>();

        transform.SetParent(this.transform);
        transform.SetAsFirstSibling();
        transform.pivot = new Vector2(0, 0);
        transform.localPosition = new Vector3(x1, y1, 0);
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y2 - y1);
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x2 - x1);
        landObject.AddComponent<CanvasRenderer>();
        landObject.AddComponent<Image>().color = color;
        landIndicators.Add(landObject);
        return landObject;
    }

    internal GameObject DrawAt(long x, long y)
    {
        GameObject obj = Add(x, x, y, y, Color.blue);
        drawnLandIndicators.Add(obj);
        return obj;
    }

    internal bool OverlapsOthers(GameObject landIndicator, Rect rect)
    {
        foreach (var li in landIndicators)
        {
            if (li != landIndicator)
            {
                var transform = li.GetComponent<RectTransform>();
                var r = transform.rect;
                if (new Rect(transform.localPosition.x, transform.localPosition.y, r.width, r.height).Overlaps(rect)) return true;
            }
        }
        return false;
    }
}
