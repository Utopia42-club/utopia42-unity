using System;
using System.Linq;
using Source.Model;
using Source.Service;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    internal class MapLandLayer : VisualElement
    {
        private readonly Map map;
        private MapLand drawingLand;
        private Vector2Int startDrawPosition;
        private readonly MapPlayerPositionIndicator mapPlayerPositionIndicator;
        private Overlay dimLayer;

        public MapLandLayer(Map map)
        {
            AddToClassList("map-land-layer");
            this.map = map;
            InitLands();
            map.RegisterCallback<PointerMoveEvent>(PointerMoved);
            map.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.ctrlKey && drawingLand == null)
                {
                    startDrawPosition = MapLand.RoundDown(this.map.ScreenToUtopia(e.mousePosition));
                    if (Children().OfType<MapLand>().Any(child => child.contentRect.Contains(startDrawPosition)))
                        return;

                    e.StopPropagation();
                    this.map.CaptureMouse();

                    drawingLand = new MapLand(new Land
                    {
                        startCoordinate = new SerializableVector3Int(startDrawPosition.x, 0, startDrawPosition.y),
                        endCoordinate = new SerializableVector3Int(startDrawPosition.x, 0, startDrawPosition.y)
                    }, map);
                    Add(drawingLand);
                }
            });
            map.RegisterCallback<MouseUpEvent>(e =>
            {
                if (!this.map.HasMouseCapture() || drawingLand == null) return;
                this.map.SubmitDrawing(drawingLand.GetLand());
                Remove(drawingLand);
                drawingLand = null;
                e.StopPropagation();
                this.map.ReleaseMouse();
            });
            map.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    var realPosition = map.ScreenToUtopia(evt.position);
                    GameManager.INSTANCE.MovePlayerTo(new Vector3(realPosition.x, 0, realPosition.y));
                }
            });
            mapPlayerPositionIndicator = new MapPlayerPositionIndicator(map);
            Add(mapPlayerPositionIndicator);
        }

        internal void SetPlayerPositionIndicatorVisibility(Visibility visibility)
        {
            mapPlayerPositionIndicator.style.visibility = visibility;
        }

        private void PointerMoved(PointerMoveEvent evt)
        {
            if (drawingLand == null) return;

            var mousePos = MapLand.RoundDown(map.ScreenToUtopia(evt.position));

            // New Rect
            var x2 = Mathf.Max(startDrawPosition.x, mousePos.x);
            var x1 = Mathf.Min(startDrawPosition.x, mousePos.x);
            var y1 = Mathf.Min(startDrawPosition.y, mousePos.y);
            var y2 = Mathf.Max(startDrawPosition.y, mousePos.y);

            ResolveCollisions(x1, x2, y1, y2);
            drawingLand.UpdateRect();
        }

        private void ResolveCollisions(int newX1, int newX2, int newY1, int newY2)
        {
            var land = drawingLand.GetLand();
            int x1 = land.startCoordinate.x;
            int x2 = land.endCoordinate.x;
            int y1 = land.startCoordinate.z;
            int y2 = land.endCoordinate.z;
            int dx1 = newX1 - x1;
            int dx2 = newX2 - x2;
            int dy1 = newY1 - y1;
            int dy2 = newY2 - y2;

            if (dx2 != 0 || dy2 != 0)
            {
                if (dx2 > 0)
                {
                    var maxX2 = ReduceLands(x2 + dx2, (ox1, ox2, oy1, oy2, maxX2) =>
                    {
                        if (ox2 > x1 && (y1 > oy1 && y1 < oy2 || y2 > oy1 && y2 < oy2
                                                              || oy1 > y1 && oy1 < y2 || oy2 > y1 && oy2 < y2))
                            return Math.Min(maxX2, ox1);
                        return maxX2;
                    });

                    x2 = Math.Max(maxX2, x2);
                }
                else x2 = Math.Max(x1, x2 + dx2);

                if (dy2 > 0)
                {
                    var maxY2 = ReduceLands(y2 + dy2, (ox1, ox2, oy1, oy2, maxY2) =>
                    {
                        if (oy2 > y1 && (x1 > ox1 && x1 < ox2 || x2 > ox1 && x2 < ox2
                                                              || ox1 > x1 && ox1 < x2 || ox2 > x1 && ox2 < x2))
                            return Math.Min(maxY2, oy1);
                        return maxY2;
                    });

                    y2 = Math.Max(maxY2, y2);
                }
                else y2 = Math.Max(y1, y2 + dy2);
            }

            if (dy1 != 0 || dx1 != 0)
            {
                if (dx1 < 0)
                {
                    var minX1 = ReduceLands(x1 + dx1, (ox1, ox2, oy1, oy2, minX1) =>
                    {
                        if (ox1 < x2 && (y1 > oy1 && y1 < oy2 || y2 > oy1 && y2 < oy2
                                                              || oy1 > y1 && oy1 < y2 || oy2 > y1 && oy2 < y2))
                            return Math.Max(minX1, ox2);
                        return minX1;
                    });

                    x1 = Math.Min(minX1, x1);
                }
                else x1 = Math.Min(x2, x1 + dx1);

                if (dy1 < 0)
                {
                    var minY1 = ReduceLands(y1 + dy1, (ox1, ox2, oy1, oy2, minY1) =>
                    {
                        if (oy1 < y2 && (x1 > ox1 && x1 < ox2 || x2 > ox1 && x2 < ox2
                                                              || ox1 > x1 && ox1 < x2 || ox2 > x1 && ox2 < x2))
                            return Math.Max(minY1, oy2);
                        return minY1;
                    });

                    y1 = Math.Min(minY1, y1);
                }
                else y1 = Math.Min(y2, y1 + dy1);
            }

            land.startCoordinate.x = x1;
            land.startCoordinate.z = y1;
            land.endCoordinate.x = x2;
            land.endCoordinate.z = y2;
        }

        /**
         * function is (indicatorX1, indicatorX2, indicatorY1, indicatorY2, current) => next. 
         * Ignores drawing land
         */
        private int ReduceLands(int seed, Func<int, int, int, int, int, int> function)
        {
            var current = seed;
            foreach (var child in Children())
            {
                if (child is not MapLand || child == drawingLand) continue;

                var land = ((MapLand) child).GetLand();
                var start = MapLand.RoundDown(land.startCoordinate.ToVector3());
                var end = MapLand.RoundUp(land.endCoordinate.ToVector3());

                current = function.Invoke(start.x, end.x, start.y, end.y, current);
            }

            return current;
        }

        private void InitLands()
        {
            var worldService = WorldService.INSTANCE;
            if (!worldService.IsInitialized()) return;

            foreach (var land in worldService.GetOwnersLands().SelectMany(entry => entry.Value))
                Add(new MapLand(land, map));
        }

        public void FocusOnLand(Land land)
        {
            dimLayer = new Overlay();
            var mapRect = this.WorldToLocal(map.worldBound);
            dimLayer.style.left = mapRect.x;
            dimLayer.style.top = mapRect.y;
            dimLayer.style.width = mapRect.width;
            dimLayer.style.height = mapRect.height;

            Add(dimLayer);
            dimLayer.BringToFront();
            foreach (var visualElement in Children())
            {
                var mapLand = visualElement as MapLand;
                if (Equals(mapLand.GetLand(), land))
                {
                    mapLand.BringToFront();
                    return;
                }
            }
        }

        public void ClearFocus()
        {
            dimLayer?.SetEnabled(false);
            dimLayer?.RemoveFromHierarchy();
        }
    }
}