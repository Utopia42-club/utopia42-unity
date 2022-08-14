using System;
using System.Collections;
using System.Collections.Generic;
using Source.Model;
using Source.Ui.Dialog;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class Map : UxmlElement
    {
        private readonly MapLandLayer lands;
        private readonly MapViewportController viewportController;
        private readonly MapPointerPositionLabel mapPointerPositionLabel;
        private readonly MapActionsLayer mapActionsLayer;
        private readonly MapLandsSearch mapLandsSearch;

        public Map() : base(typeof(Map), true)
        {
            var root = this.Q("MapRoot");
            var grid = new MapGrid(this);
            root.Add(grid);
            root.Add(lands = new MapLandLayer(this));
            mapPointerPositionLabel = new MapPointerPositionLabel(this);
            root.Add(mapPointerPositionLabel);

            viewportController = new MapViewportController(this, e =>
            {
                lands.transform.position = new Vector3(-e.rect.x, -e.rect.y, 0);
                lands.transform.scale = new Vector3(e.scale, e.scale, 1);
                grid.UpdateViewport(e.scale);
            });

            mapActionsLayer = new MapActionsLayer(this);
            root.Add(mapActionsLayer);
            mapLandsSearch = new MapLandsSearch(this);
            root.Add(mapLandsSearch);

            RegisterCallback<GeometryChangedEvent>(evt => MoveToPlayerPosition());
        }

        internal void MoveToPlayerPosition()
        {
            var pos = Player.INSTANCE.GetPosition();
            MoveTo(new Vector2(pos.x, pos.z));
        }

        internal void MoveTo(Vector2 pos)
        {
            viewportController.MoveToPosition(pos);
        }

        internal void MoveTo(Land land)
        {
            var width = land.endCoordinate.x - land.startCoordinate.x;
            var height = land.endCoordinate.z - land.startCoordinate.z;
            viewportController.MoveToPosition(
                new Vector2(land.startCoordinate.x + width / 2, land.startCoordinate.z + height / 2));
        }

        internal Vector2 ScreenToUtopia(Vector2 pos)
        {
            var local = lands.WorldToLocal(pos);
            return new Vector2(local.x, -local.y);
        }

        internal Vector2 UtopiaToScreen(Vector2 pos)
        {
            return lands.LocalToWorld(new Vector2(pos.x, -pos.y));
        }

        public void SubmitDrawing(Land land)
        {
            var buyDialog = new LandBuyDialog(land);
            DialogService.INSTANCE.Show(
                new DialogConfig(buyDialog)
                    .WithWidth(new StyleLength(new Length(300)))
                    .WithHeight(new StyleLength(new Length(180)))
                    .WithCancelAction()
                    .WithAction(new DialogAction("Buy",
                        () => GameManager.INSTANCE.Buy(new List<Land> {land})
                        , "utopia-button-secondary"))
            );
        }

        public void ZoomIn()
        {
            viewportController.ZoomIn();
        }

        public void ZoomOut()
        {
            viewportController.ZoomOut();
        }

        public IEnumerator TakeNftScreenShot(Land land, Action<byte[]> consumer)
        {
            // Preparing map for screen shot
            mapPointerPositionLabel.style.display = DisplayStyle.None;
            mapLandsSearch.style.visibility = Visibility.Hidden;
            mapActionsLayer.style.visibility = Visibility.Hidden;
            lands.SetPlayerPositionIndicatorVisibility(Visibility.Hidden);
            viewportController.BackToDefaultZoom();
            MoveTo(land);
            DialogService.INSTANCE.CloseAll();
            lands.FocusOnLand(land);
            yield return new WaitForEndOfFrame();

            var width = (int) worldBound.width - 1;
            var height = (int) worldBound.height - 1;
            var screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            screenshot.ReadPixels(new Rect(worldBound.xMin, 0, width, height), 0, 0);
            screenshot.Apply();
            yield return null;

            consumer.Invoke(screenshot.EncodeToPNG());
            GameManager.Destroy(screenshot);

            mapPointerPositionLabel.style.display = DisplayStyle.Flex;
            mapLandsSearch.style.visibility = Visibility.Visible;
            mapActionsLayer.style.visibility = Visibility.Visible;
            lands.SetPlayerPositionIndicatorVisibility(Visibility.Visible);
            lands.ClearFocus();
        }

        public void CloseSearchPanelIfOpened()
        {
            if (mapLandsSearch.IsLandsListOpen())
                mapLandsSearch.ToggleLandsList();
        }
    }
}