using System.Collections.Generic;
using src.MetaBlocks.MarkerBlock;
using src.Model;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class OwnedLandsDialogContent : MonoBehaviour
    {
        public static readonly string PREFAB = "OwnedLands";
        public static readonly string LAND_VIEW_PREAB = "LandView";

        private readonly List<GameObject> landObjects = new List<GameObject>();

        public GameObject landsList;

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