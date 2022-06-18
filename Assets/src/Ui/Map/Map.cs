using UnityEngine;
using UnityEngine.UIElements;

namespace src.Ui.Map
{
    public class Map : UxmlElement
    {
        private readonly VisualElement lands;
        private readonly MapViewportController viewportController;

        public Map() : base("UiDocuments/Map/Map", true)
        {
            lands = this.Q("Lands");
            var grid = new MapGrid();
            Add(grid);
            viewportController = new MapViewportController(this, e =>
            {
                lands.transform.position = new Vector3(-e.rect.x, -e.rect.y, 0);
                lands.transform.scale = new Vector3(e.scale, e.scale, 1);
                grid.UpdateViewport(e);
            });
        }

        internal Vector2 ScreenToUtopia(Vector2 pos)
        {
            return lands.WorldToLocal(pos);
        }

        internal Vector2 UtopiaToScreen(Vector2 pos)
        {
            return lands.LocalToWorld(pos);
        }
    }
}