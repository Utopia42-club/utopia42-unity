using System.Collections.Generic;
using System.Linq;
using Source.Model;
using Source.Service;
using Source.Ui.Popup;
using Source.Ui.Utils;
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
        private bool isLandsListOpen = false;
        private PopupController searchPopupConttoller;
        private MapLandsList popupLandsList;
        private readonly Button saveButton;

        public MapLandsSearch(Map map) : base(typeof(MapLandsSearch))
        {
            myLands = this.Q<VisualElement>("myLands");
            myLands.style.width = 0;
            myLands.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());

            searchBox = this.Q<VisualElement>("searchBox");
            landsListContainer = this.Q<VisualElement>("landsListContainer");
            menuButton = this.Q<Button>("menuButton");
            menuButton.clickable.clicked += ToggleLandsList;

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
                if (isLandsListOpen)
                    mapLandsList.SetLands(FilterLands(WorldService.INSTANCE.GetPlayerLands()));
                else
                {
                    if (string.IsNullOrEmpty(searchField.value) && searchPopupConttoller != null)
                    {
                        searchPopupConttoller.Close();
                        return;
                    }

                    if (searchPopupConttoller != null)
                    {
                        popupLandsList.SetLands(FilterLands(GetAllLands()));
                    }
                    else
                    {
                        popupLandsList = new MapLandsList(map);
                        searchPopupConttoller =
                            PopupService.INSTANCE.Show(
                                new PopupConfig(popupLandsList, searchBox, Side.Bottom)
                                    .WithWidth(300)
                                    .WithHeight(400)
                                    .WithOnClose(() => searchPopupConttoller = null));
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

        public void ToggleLandsList()
        {
            isLandsListOpen = !isLandsListOpen;
            if (isLandsListOpen)
            {
                mapLandsList.SetLands(WorldService.INSTANCE.GetPlayerLands());
                myLands.style.width = 300;
                myLands.style.display = DisplayStyle.Flex;
            }
            else
            {
                myLands.style.width = 0;
                myLands.schedule.Execute(() => { myLands.style.display = DisplayStyle.None; }).StartingIn(500);
            }
        }

        public bool IsLandsListOpen()
        {
            return isLandsListOpen;
        }

        private List<Land> FilterLands(List<Land> lands)
        {
            return lands.Where(land =>
            {
                var name = land.GetName() ?? "";
                return name.ToUpper().Contains(searchField.text.ToUpper()) ||
                       land.id.ToString().Contains(searchField.text);
            }).ToList();
        }
    }
}