using System.Linq;
using Source.Model;
using Source.Service;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    internal class MapLandLayer : VisualElement
    {
        public MapLandLayer()
        {
            InitLands();
        }

        private void InitLands()
        {
            Add(new MapLand(new Land()
            {
                id = 1, owner = "xyz", startCoordinate = new SerializableVector3Int(Vector3Int.zero),
                endCoordinate = new SerializableVector3Int(Vector3Int.one)
            }));
            // var worldService = WorldService.INSTANCE;
            // if (!worldService.IsInitialized()) return;
            //
            // foreach (var land in worldService.GetOwnersLands().SelectMany(entry => entry.Value))
            //     Add(new MapLand(land));
        }
    }
}