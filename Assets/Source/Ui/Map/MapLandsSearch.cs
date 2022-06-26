using System.Collections.Generic;
using System.Linq;
using Source.Model;
using Source.Service;
using Source.Ui.Popup;
using UnityEngine.UIElements;

namespace Source.Ui.Map
{
    public class MapLandsSearch : UxmlElement
    {
        private readonly VisualElement myLands;
        private readonly VisualElement searchBox;
        private readonly VisualElement landsListContainer;
        private readonly Button menuButton;
        private readonly TextField searchField;
        private readonly MapLandsList mapLandsList;
        private bool isListOpen = false;
        private int? searchPopupId;
        private MapLandsList popupLandsList;

        public MapLandsSearch(Map map) : base("Ui/Map/MapLandsSearch")
        {
            myLands = this.Q<VisualElement>("myLands");
            searchBox = this.Q<VisualElement>("searchBox");
            landsListContainer = this.Q<VisualElement>("landsListContainer");
            menuButton = this.Q<Button>("menuButton");
            searchField = this.Q<TextField>("searchField");
            Utils.Utils.SetPlaceHolderForTextField(searchField, "Search");
            Utils.Utils.RegisterUiEngagementCallbacksForTextField(searchField);

            menuButton.clickable.clicked += ToggleMyLandsList;
            mapLandsList = new MapLandsList(map);
            landsListContainer.Add(mapLandsList);

            searchField.RegisterValueChangedCallback(evt =>
            {
                if (isListOpen)
                    mapLandsList.SetLands(FilterLands(WorldService.INSTANCE.GetPlayerLands()));
                else
                {
                    if (string.IsNullOrEmpty(searchField.value) && searchPopupId != null)
                    {
                        PopupService.INSTANCE.Close(searchPopupId.Value);
                        searchPopupId = null;
                        return;
                    }

                    if (searchPopupId != null)
                    {
                        //FIXME search between all lands
                        popupLandsList.SetLands(FilterLands(WorldService.INSTANCE.GetPlayerLands()));
                    }
                    else
                    {
                        popupLandsList = new MapLandsList(map);
                        searchPopupId =
                            PopupService.INSTANCE.Show(
                                new PopupConfig(popupLandsList, searchField, Side.Bottom)
                                    .WithWidth(300)
                                    .WithHeight(400)
                            );
                        //FIXME search between all lands
                        popupLandsList.SetLands(FilterLands(WorldService.INSTANCE.GetPlayerLands()));
                    }
                }
            });
        }

        private void ToggleMyLandsList()
        {
            myLands.style.display = isListOpen ? DisplayStyle.None : DisplayStyle.Flex;
            isListOpen = !isListOpen;
            if (isListOpen)
                mapLandsList.SetLands(WorldService.INSTANCE.GetPlayerLands());
        }

        private List<Land> FilterLands(List<Land> lands)
        {
            return lands.Where(land =>
            {
                var name = land.GetName() ?? "";
                return name.Contains(searchField.text) || land.id.ToString().Contains(searchField.text);
            }).ToList();
        }
    }
}