using System;
using System.Collections.Generic;
using src.Model;
using src.Service;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class RectPane : MonoBehaviour
    {
        [SerializeField] internal Transform landContainer;
        [SerializeField] private RectTransform playerPosIndicator;
        private readonly Dictionary<long, GameObject> landIndicators = new Dictionary<long, GameObject>();
        private readonly HashSet<GameObject> drawnLandIndicators = new HashSet<GameObject>();


        [SerializeField] public GameObject landPrefab;

        public SelectionHandler selectedLand;
        private Land targetLand;

        void Start()
        {
            var manager = GameManager.INSTANCE;
            if (manager.GetState() == GameManager.State.MAP)
                Init();

            manager.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.MAP)
                    Init();
                else
                {
                    DestroyRects();
                }
            });
        }

        private void Init()
        {
            WorldService service = WorldService.INSTANCE;
            if (!service.IsInitialized()) return;
            landContainer.localScale = Vector3.one;

            if (targetLand == null)
            {
                var playerPos = Player.INSTANCE.transform.position;
                playerPosIndicator.localPosition = new Vector3(playerPos.x, playerPos.z, 0);
                var transform = GetComponent<RectTransform>();
                transform.anchoredPosition = new Vector3(-playerPos.x, -playerPos.z, 0);
            }
            else
                targetLand = null;

            foreach (var entry in service.GetOwnersLands())
            {
                var owner = entry.Key.Equals(Settings.WalletId());
                foreach (var land in entry.Value)
                    Add(land.startCoordinate.x, land.endCoordinate.x, land.startCoordinate.z, land.endCoordinate.z,
                        owner ? land.isNft ? Colors.MAP_OWNED_LAND_NFT : Colors.MAP_OWNED_LAND :
                        land.isNft ? Colors.MAP_OTHERS_LAND_NFT : Colors.MAP_OTHERS_LAND, land, entry.Key);
            }
        }

        private void DestroyRects()
        {
            foreach (var lo in landIndicators.Values)
                DestroyImmediate(lo);
            landIndicators.Clear();
            drawnLandIndicators.Clear();
        }

        private GameObject Add(int x1, int x2, int y1, int y2, Color color, Land land, string walletId)
        {
            var landObject = Instantiate(landPrefab);

            var selectionHandler = landObject.GetComponent<SelectionHandler>();
            selectionHandler.land = land;
            selectionHandler.walletId = walletId;
            selectionHandler.rectPane = this;
            if (land != null)
                landIndicators[land.id] = landObject;

            const int outlineWidth = 4;
            var newX1 = x1 + outlineWidth;
            var newX2 = x2 - outlineWidth;
            var newY1 = y1 + outlineWidth;
            var newY2 = y2 - outlineWidth;

            var landTransform = landObject.GetComponent<RectTransform>();
            landTransform.SetParent(landContainer);
            landTransform.SetAsFirstSibling();
            landTransform.pivot = new Vector2(0, 0);
            landTransform.localPosition = new Vector3(newX1, newY1, 0);
            landTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newY2 - newY1);
            landTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newX2 - newX1);

            var outline = landObject.GetComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(outlineWidth, outlineWidth);

            landObject.GetComponent<Image>().color = Colors.GetLandColor(land);
            
            const int nftLogoDefaultSize = 30;
            var nftLogo = landObject.transform.Find("NftLogo").gameObject.GetComponent<Image>();
            nftLogo.gameObject.SetActive(land != null && land.isNft);
            var nftLogoTransform = nftLogo.GetComponent<RectTransform>();
            nftLogoTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Math.Min(newY2 - newY1, nftLogoDefaultSize));
            nftLogoTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Math.Min(newX2 - newX1, nftLogoDefaultSize));

            return landObject;
        }

        private void UpdateLandColor(Land land)
        {
            landIndicators[selectedLand.land.id].GetComponent<Image>().color = Colors.GetLandColor(land);
        }

        internal void DeleteDrawingObject(GameObject drawingObject)
        {
            DestroyImmediate(drawingObject);
            drawnLandIndicators.Remove(drawingObject);
        }

        internal GameObject DrawAt(int x, int y)
        {
            GameObject obj = Add(x, x, y, y, Colors.PRIMARY_COLOR, null, Settings.WalletId());
            drawnLandIndicators.Add(obj);
            return obj;
        }

        internal Rect ResolveCollisions(GameObject landIndicator, int x1, int x2, int y1, int y2, int dx1, int dx2,
            int dy1, int dy2)
        {
            if (dx2 != 0 || dy2 != 0)
            {
                if (dx2 > 0)
                {
                    var maxX2 = ForEachIndicator(x2 + dx2, (ox1, ox2, oy1, oy2, maxX2) =>
                    {
                        if (ox2 > x1 && (y1 > oy1 && y1 < oy2 || y2 > oy1 && y2 < oy2
                                                              || oy1 > y1 && oy1 < y2 || oy2 > y1 && oy2 < y2))
                            return Math.Min(maxX2, ox1);
                        return maxX2;
                    }, landIndicator);

                    x2 = Math.Max(maxX2, x2);
                }
                else x2 = Math.Max(x1, x2 + dx2);

                if (dy2 > 0)
                {
                    var maxY2 = ForEachIndicator(y2 + dy2, (ox1, ox2, oy1, oy2, maxY2) =>
                    {
                        if (oy2 > y1 && (x1 > ox1 && x1 < ox2 || x2 > ox1 && x2 < ox2
                                                              || ox1 > x1 && ox1 < x2 || ox2 > x1 && ox2 < x2))
                            return Math.Min(maxY2, oy1);
                        return maxY2;
                    }, landIndicator);

                    y2 = Math.Max(maxY2, y2);
                }
                else y2 = Math.Max(y1, y2 + dy2);
            }

            if (dy1 != 0 || dx1 != 0)
            {
                if (dx1 < 0)
                {
                    var minX1 = ForEachIndicator(x1 + dx1, (ox1, ox2, oy1, oy2, minX1) =>
                    {
                        if (ox1 < x2 && (y1 > oy1 && y1 < oy2 || y2 > oy1 && y2 < oy2
                                                              || oy1 > y1 && oy1 < y2 || oy2 > y1 && oy2 < y2))
                            return Math.Max(minX1, ox2);
                        return minX1;
                    }, landIndicator);

                    x1 = Math.Min(minX1, x1);
                }
                else x1 = Math.Min(x2, x1 + dx1);

                if (dy1 < 0)
                {
                    var minY1 = ForEachIndicator(y1 + dy1, (ox1, ox2, oy1, oy2, minY1) =>
                    {
                        if (oy1 < y2 && (x1 > ox1 && x1 < ox2 || x2 > ox1 && x2 < ox2
                                                              || ox1 > x1 && ox1 < x2 || ox2 > x1 && ox2 < x2))
                            return Math.Max(minY1, oy2);
                        return minY1;
                    }, landIndicator);

                    y1 = Math.Min(minY1, y1);
                }
                else y1 = Math.Min(y2, y1 + dy1);
            }

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }


        /*
         * function is (indicatorX1, indicatorX2, indicatorY1, indicatorY2, current) => next
         */
        private int ForEachIndicator(int seed, Func<int, int, int, int, int, int> function, GameObject ignore)
        {
            var current = seed;
            foreach (var li in landIndicators.Values)
            {
                if (li != ignore)
                {
                    var transform = li.GetComponent<RectTransform>();
                    var or = transform.rect;
                    var olp = transform.localPosition;
                    var x1 = MapInputManager.RoundDown((int) olp.x);
                    var x2 = MapInputManager.RoundUp(olp.x + (int) or.width);
                    var y1 = MapInputManager.RoundDown((int) olp.y);
                    var y2 = MapInputManager.RoundUp(olp.y + (int) or.height);

                    current = function.Invoke(x1, x2, y1, y2, current);
                }
            }

            return current;
        }

        internal List<Land> GetDrawn()
        {
            var drawn = new List<Land>();
            foreach (var indicator in drawnLandIndicators)
            {
                var transform = indicator.GetComponent<RectTransform>();
                var r = transform.rect;
                var land = new Land();

                var localPosition = transform.localPosition;
                land.startCoordinate = new SerializableVector3Int((int) localPosition.x, 0, (int) localPosition.y);
                land.endCoordinate = new SerializableVector3Int(land.startCoordinate.x + (int) r.width, 0,
                    land.startCoordinate.z + (int) r.height);
                drawn.Add(land);
            }

            return drawn;
        }

        internal bool HasDrawn()
        {
            return drawnLandIndicators.Count != 0;
        }

        public void HidePlayerPosIndicator()
        {
            playerPosIndicator.gameObject.SetActive(false);
        }

        public void ShowPlayerPosIndicator()
        {
            playerPosIndicator.gameObject.SetActive(true);
        }

        internal void OpenDialogForLand(SelectionHandler selectionHandler)
        {
            if (selectedLand != null && selectionHandler != selectedLand)
                selectedLand.SetSelected(false, true);
            selectedLand = selectionHandler;
            if (!selectedLand) return;
            var landProfileDialog = LandProfileDialog.INSTANCE;
            landProfileDialog.Open(selectedLand.land, Profile.LOADING_PROFILE);
            landProfileDialog.WithOneClose(() => { UpdateLandColor(selectedLand.land); });
            ProfileLoader.INSTANCE.load(selectedLand.walletId, landProfileDialog.SetProfile,
                () => landProfileDialog.SetProfile(Profile.FAILED_TO_LOAD_PROFILE));
        }

        public void SetTargetLand(Land land)
        {
            targetLand = land;
        }
    }
}