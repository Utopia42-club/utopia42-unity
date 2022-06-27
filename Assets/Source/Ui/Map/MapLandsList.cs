using System.Collections.Generic;
using Source.Model;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapLandsList : ScrollView
    {
        private readonly Map map;
        private List<Land> lands;

        public MapLandsList(Map map)
        {
            this.map = map;
            Utils.Utils.IncreaseScrollSpeed(this, 600);
            contentContainer.style.paddingBottom = contentContainer.style.paddingTop =
                contentContainer.style.paddingLeft = contentContainer.style.paddingRight = 5;
        }

        public void SetLands(List<Land> lands)
        {
            this.lands = lands;
            contentContainer.Clear();
            foreach (var land in lands)
            {
                var element = new MapLandListItemView(land, map)
                {
                    style =
                    {
                        marginBottom = 10
                    }
                };
                contentContainer.Add(element);
            }
        }
    }
}