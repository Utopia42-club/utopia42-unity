using System.Collections.Generic;
using Source.Model;
using Source.Ui.Dialog;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class Map : UxmlElement
    {
        private readonly VisualElement lands;
        private readonly MapViewportController viewportController;

        public Map() : base(true)
        {
            var root = this.Q("Root");
            var grid = new MapGrid(this);
            root.Add(grid);
            root.Add(lands = new MapLandLayer(this));
            root.Add(new MapPointerPositionLabel(this));

            viewportController = new MapViewportController(this, e =>
            {
                lands.transform.position = new Vector3(-e.rect.x, -e.rect.y, 0);
                lands.transform.scale = new Vector3(e.scale, e.scale, 1);
                grid.UpdateViewport(e.scale);
            });

            root.Add(new MapActionsLayer(this));
            root.Add(new MapLandsSearch(this));

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
                        , "utopia-stroked-button-secondary"))
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
    }
}