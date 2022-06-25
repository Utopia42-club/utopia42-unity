using System.Collections.Generic;
using Source.Model;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapLandsList : ScrollView
    {
        private List<Land> lands;

        public void SetItems(List<Land> lands)
        {
            this.lands = lands;
            contentContainer.Clear();
            foreach (var land in lands)
                contentContainer.Add(new MapLandListItemView(land));
        }
    }
}