using System;
using Source.Canvas;
using Source.Model;
using Source.Ui.Dialog;
using Source.Ui.Loading;
using Source.Ui.Profile;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Position = UnityEngine.UIElements.Position;

namespace Source.Ui.Map
{
    internal class MapLand : VisualElement
    {
        private readonly Land land;
        private static readonly Sprite nftLogo = Resources.Load<Sprite>("Icons/nft-logo");
        private readonly Map map;
        private readonly VisualElement backgroundLayer;

        public MapLand(Land land, Map map)
        {
            this.land = land;
            this.map = map;
            

            UpdateLandStyle();
            AddToClassList("map-land");
            Add(backgroundLayer = new VisualElement());
            backgroundLayer.AddToClassList("map-land-background-layer");
            UpdateRect();

            if (land is {isNft: true})
            {
                const int nftLogoDefaultSize = 30;
                var width = style.width.value.value;
                var height = style.height.value.value;
                var visualElement = new VisualElement
                {
                    style =
                    {
                        width = Math.Min(width - 6, nftLogoDefaultSize), // -6 is for border and position
                        height = Math.Min(height - 6, nftLogoDefaultSize), // -6 is for border and position
                        position = new StyleEnum<Position>(Position.Absolute),
                        bottom = 2,
                        right = 2
                    }
                };
                UiImageUtils.SetBackground(visualElement, nftLogo, false);
                Add(visualElement);
            }

            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int) MouseButton.RightMouse)
                {
                    var landProfile = new LandProfile(map, land);
                    var controller = DialogService.INSTANCE.Show(new DialogConfig("Land Profile", landProfile)
                        .WithWidth(new Length(100, LengthUnit.Percent))
                        .WithHeight(new Length(100, LengthUnit.Percent))
                        .WithOnClose(UpdateLandStyle));
                    var loading = LoadingLayer.Show(controller.Dialog);
                    ProfileLoader.INSTANCE.load(land.owner, profile =>
                        {
                            loading.Close();
                            landProfile.SetProfile(land.owner, profile);
                        },
                        () =>
                        {
                            loading.Close();
                            landProfile.SetProfile(land.owner, Model.Profile.FAILED_TO_LOAD_PROFILE);
                        });
                }
            });
        }

        private void UpdateLandStyle()
        {
            if (land.owner == null)
            {
                AddToClassList("map-new-drawing-land");
                return;
            }

            var landColor = Colors.GetLandColor(land);
            if (landColor != null)
                backgroundLayer.style.backgroundColor = new StyleColor(landColor.Value);
            AddToClassList(Colors.GetLandBorderStyle(land));
        }

        internal void UpdateRect()
        {
            var end = land.endCoordinate.ToVector3();
            var start = land.startCoordinate.ToVector3();
            var diag = end - start;
            style.top = -end.z;
            style.left = start.x;
            style.width = diag.x;
            style.height = diag.z;
        }

        internal Land GetLand()
        {
            return land;
        }

        internal static Vector2Int RoundDown(Vector2 v)
        {
            return new Vector2Int(RoundDown(v.x), RoundDown(v.y));
        }

        internal static Vector3Int RoundDown(Vector3 v)
        {
            return new Vector3Int(RoundDown(v.x), RoundDown(v.y), RoundDown(v.z));
        }

        internal static Vector3Int RoundUp(Vector3 v)
        {
            return new Vector3Int(RoundUp(v.x), RoundUp(v.y), RoundUp(v.z));
        }

        internal static int RoundDown(float x)
        {
            return 5 * (int) Mathf.Floor(x / 5);
        }

        internal static int RoundUp(float x)
        {
            return 5 * (int) Mathf.Ceil(x / 5);
        }
    }
}