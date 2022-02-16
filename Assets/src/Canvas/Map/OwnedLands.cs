using System.Collections.Generic;
using src.Model;
using src.Service;
using UnityEngine;

namespace src.Canvas.Map
{
    public class OwnedLands : MonoBehaviour
    {
        public static readonly string PREFAB = "OwnedLands";
        public static readonly string LAND_VIEW_PREAB = "LandView";

        private readonly List<GameObject> landObjects = new List<GameObject>();

        public GameObject landsList;

        private void Start()
        {
            SetLands(WorldService.INSTANCE.GetLandsFor(Settings.WalletId()));
        }

        public void SetLands(List<Land> lands)
        {
            if (lands == null)
                return;
            foreach (var land in lands)
            {
                var l = Instantiate(Resources.Load<GameObject>(LAND_VIEW_PREAB), landsList.transform);
                l.GetComponent<LandView>().SetLand(land);
                landObjects.Add(l.gameObject);
            }
        }

        public void Close()
        {
            if (landObjects.Count > 0)
            {
                foreach (var land in landObjects) DestroyImmediate(land);
                landObjects.Clear();
            }

            gameObject.SetActive(false);
        }
    }
}