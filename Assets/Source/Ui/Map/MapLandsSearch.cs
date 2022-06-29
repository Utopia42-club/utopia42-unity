using System.Collections.Generic;
using System.Linq;
using Source.Model;
using Source.Service;
using Source.Ui.Popup;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Position = UnityEngine.UIElements.Position;

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
        private readonly Button saveButton;

        public MapLandsSearch(Map map) : base("Ui/Map/MapLandsSearch")
        {
            myLands = this.Q<VisualElement>("myLands");
            // myLands.style.width = 0;
            myLands.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());

            searchBox = this.Q<VisualElement>("searchBox");
            landsListContainer = this.Q<VisualElement>("landsListContainer");
            menuButton = this.Q<Button>("menuButton");
            menuButton.clickable.clicked += ToggleMyLandsList;

            saveButton = this.Q<Button>("saveLandsButton");
            saveButton.clickable.clicked += () => GameManager.INSTANCE.Save();
            saveButton.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
            
            searchField = this.Q<TextField>("searchField");
            TextFields.SetPlaceHolderForTextField(searchField, "Search");
            TextFields.RegisterUiEngagementCallbacksForTextField(searchField);

            mapLandsList = new MapLandsList(map);
            mapLandsList.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
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
                        return;
                    }

                    if (searchPopupId != null)
                    {
                        popupLandsList.SetLands(FilterLands(GetAllLands()));
                    }
                    else
                    {
                        popupLandsList = new MapLandsList(map);
                        searchPopupId =
                            PopupService.INSTANCE.Show(
                                new PopupConfig(popupLandsList, searchBox, Side.Bottom)
                                    .WithWidth(300)
                                    .WithHeight(400)
                                    .WithOnClose(() => searchPopupId = null));
                        popupLandsList.SetLands(FilterLands(GetAllLands()));
                    }
                }
            });

            style.position = new StyleEnum<Position>(Position.Absolute);
            style.top = style.left = style.bottom = 0;
            style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        }

        private static List<Land> GetAllLands()
        {
            return WorldService.INSTANCE.GetOwnersLands()
                .SelectMany(entry => entry.Value).ToList();
        }

        private void ToggleMyLandsList()
        {
            // saveButton.SetEnabled(WorldService.INSTANCE.HasChange());
            myLands.style.display = isListOpen ? DisplayStyle.None : DisplayStyle.Flex;
            isListOpen = !isListOpen;
            if (isListOpen)
            {
                mapLandsList.SetLands(WorldService.INSTANCE.GetPlayerLands());
                // myLands.style.width = 300;
            }
            else
            {
                // myLands.style.width = 0;
            }
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