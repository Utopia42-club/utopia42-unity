using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectPane : MonoBehaviour
{
    [SerializeField] private RectTransform playerPosIndicator;
    private readonly HashSet<GameObject> landIndicators = new HashSet<GameObject>();
    private readonly HashSet<GameObject> drawnLandIndicators = new HashSet<GameObject>();

    [SerializeField] public GameObject landPrefab;
    [SerializeField] public Button transferButton;

    public TransferHandler selectedLand;

    void Start()
    {
        var manager = GameManager.INSTANCE;
        if (manager.GetState() == GameManager.State.MAP) Init();

        manager.stateChange.AddListener(s =>
        {
            if (s == GameManager.State.MAP) Init();
            else DestroyRects();
        });

        transferButton.onClick.AddListener(DoTransfer);
    }

    private void DoTransfer()
    {
        GameManager.INSTANCE.Transfer(selectedLand.landId);
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
                Add(land.x1, land.x2, land.y1, land.y2, c, land.id, entry.Key);
        }
    }

    private void DestroyRects()
    {
        foreach (var lo in landIndicators)
            DestroyImmediate(lo);
        landIndicators.Clear();
        drawnLandIndicators.Clear();
    }


    internal void setSelected(TransferHandler transferHandler)
    {
        if (selectedLand != null && transferHandler != selectedLand)
            selectedLand.setSelected(false, true);
        selectedLand = transferHandler;
        transferButton.gameObject.SetActive(selectedLand != null && selectedLand.walletId.Equals(Settings.WalletId()));
    }

    private GameObject Add(long x1, long x2, long y1, long y2, Color color, long landId, string walletId)
    {
        var landObject = Instantiate(landPrefab);
        var transform = landObject.GetComponent<RectTransform>();
        TransferHandler transferHandler = landObject.GetComponent<TransferHandler>();
        transferHandler.landId = landId;
        transferHandler.walletId = walletId;
        transferHandler.rectPane = this;

        transform.SetParent(this.transform);
        transform.SetAsFirstSibling();
        transform.pivot = new Vector2(0, 0);
        transform.localPosition = new Vector3(x1, y1, 0);
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y2 - y1);
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x2 - x1);
        landObject.GetComponent<Image>().color = color;
        landIndicators.Add(landObject);
        return landObject;
    }

    internal void Delete(GameObject drawingObject)
    {
        DestroyImmediate(drawingObject);
        drawnLandIndicators.Remove(drawingObject);
        landIndicators.Remove(drawingObject);
    }

    internal GameObject DrawAt(long x, long y)
    {
        GameObject obj = Add(x, x, y, y, Color.blue, -1, Settings.WalletId());
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
                if (new Rect(transform.localPosition.x, transform.localPosition.y, r.width, r.height).Overlaps(rect))
                    return true;
            }
        }

        return false;
    }

    internal List<Land> GetDrawn()
    {
        var drawn = new List<Land>();
        foreach (var indicator in drawnLandIndicators)
        {
            var transform = indicator.GetComponent<RectTransform>();
            var r = transform.rect;
            var land = new Land();
            land.x1 = (long) transform.localPosition.x;
            land.y1 = (long) transform.localPosition.y;
            land.x2 = land.x1 + (long) r.width;
            land.y2 = land.y1 + (long) r.height;
            drawn.Add(land);
        }

        return drawn;
    }

    internal bool HasDrawn()
    {
        return drawnLandIndicators.Count != 0;
    }
}