using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class Map : UxmlElement
    {
        private readonly VisualElement lands;
        private readonly MapViewportController viewportController;
        private readonly VisualElement root;

        public Map() : base(true)
        {
            root = this.Q("Root");
            root.Add(lands = new MapLandLayer());
            var grid = new MapGrid(this);
            root.Add(grid);
            root.Add(new MapPointerPositionLabel(this));

            viewportController = new MapViewportController(this, e =>
            {
                lands.transform.position = new Vector3(-e.rect.x, -e.rect.y, 0);
                lands.transform.scale = new Vector3(e.scale, e.scale, 1);
                grid.UpdateViewport(e.scale);
                Debug.Log(e.rect.x / e.scale + ", " + e.rect.y / e.scale);
            });
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
    }
}