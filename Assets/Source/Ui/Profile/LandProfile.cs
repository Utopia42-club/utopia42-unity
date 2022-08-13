using Source.Canvas;
using Source.Model;
using Source.Service;
using Source.Service.Auth;
using Source.Ui.CustomUi;
using Source.Ui.Popup;
using UnityEngine.UIElements;

namespace Source.Ui.Profile
{
    public class LandProfile : UxmlElement
    {
        private readonly Map.Map map;
        private static readonly string colorPickerPrefab = "Prefabs/FloatColorPicker";

        private Label nameLabel;
        private TextField nameField;
        private Label idLabel;
        private Label sizeLabel;
        private VisualElement colorValue;
        private VisualElement actions;
        private VisualElement nftLogo;
        private Button editButton;
        private Button transferButton;
        private Button toggleNftButton;
        private readonly UserProfile userProfile;
        private bool editMode = false;
        private readonly EventCallback<MouseDownEvent> colorValueClickCallback;
        private Land land;
        private readonly VisualElement userProfileContainer;

        public LandProfile(Map.Map map, Land land) : base(typeof(LandProfile), true)
        {
            this.map = map;
            userProfileContainer = this.Q<VisualElement>("userProfileContainer");
            userProfile = new UserProfile();
            userProfileContainer.Add(userProfile);
            editButton = this.Q<Button>("editButton");
            idLabel = this.Q<Label>("idValue");
            nameLabel = this.Q<Label>("nameValue");
            nameField = this.Q<TextField>("nameField");
            sizeLabel = this.Q<Label>("sizeValue");
            colorValue = this.Q<VisualElement>("colorValue");
            nftLogo = this.Q<VisualElement>("nftLogo");
            actions = this.Q<VisualElement>("actions");
            transferButton = this.Q<Button>("transferButton");
            toggleNftButton = this.Q<Button>("toggleNftButton");
            transferButton.clickable.clicked += () => GameManager.INSTANCE.Transfer(land.id);
            toggleNftButton.clickable.clicked += () => GameManager.INSTANCE.SetNFT(map, land, !land.isNft);
            SetLand(land);
            editButton.clicked += OnEditButtonClicked;
            colorValueClickCallback = evt =>
            {
                PopupController popupController = null;
                var colorPicker = new ColorPicker(color =>
                {
                    colorValue.style.backgroundColor = new StyleColor(color);
                    var newColor = Colors.ConvertToHex(color);
                    land.properties ??= new LandProperties();
                    land.properties.color = newColor;
                    WorldService.INSTANCE.UpdateLandProperties(land.id, land.properties);
                    popupController.Close();
                });
                colorPicker.SetColor(Colors.GetLandColor(land) ?? Colors.MAP_DEFAULT_LAND_COLOR);
                popupController = PopupService.INSTANCE.Show(new PopupConfig(colorPicker, colorValue, Side.BottomLeft)
                    .WithWidth(250));
            };
            nameField.RegisterValueChangedCallback(evt =>
            {
                land.properties ??= new LandProperties();
                land.properties.name = editMode ? nameField.text : nameLabel.text;
                WorldService.INSTANCE.UpdateLandProperties(land.id, land.properties);
            });
        }

        private void OnEditButtonClicked()
        {
            editMode = !editMode;
            if (editMode)
                nameField.value = nameLabel.text;
            else
                nameLabel.text = nameField.value;
            nameField.style.display = editMode ? DisplayStyle.Flex : DisplayStyle.None;
            nameLabel.style.display = editMode ? DisplayStyle.None : DisplayStyle.Flex;
            editButton.text = editMode ? "Done" : "Edit";
            if (editMode)
                colorValue.RegisterCallback(colorValueClickCallback);
            else
                colorValue.UnregisterCallback(colorValueClickCallback);
        }

        private void SetLand(Land land)
        {
            this.land = land;
            if (land == null)
                return;
            idLabel.text = land.id.ToString();
            var rect = land.ToRect();
            sizeLabel.text = (rect.width * rect.height).ToString();
            nftLogo.style.display = land.isNft ? DisplayStyle.Flex : DisplayStyle.None;
            transferButton.style.display = land.isNft ? DisplayStyle.None : DisplayStyle.Flex;
            var isOwner = AuthService.Instance.IsCurrentUser(land.owner);
            actions.style.display = isOwner ? DisplayStyle.Flex : DisplayStyle.None;

            var landColor = Colors.GetLandColor(land);
            if (landColor != null)
                colorValue.style.backgroundColor = new StyleColor(landColor.Value);

            if (isOwner)
                toggleNftButton.text = land.isNft ? "Remove NFT" : "Make NFT";

            nameLabel.text = land.GetName();

            editButton.style.display = isOwner ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetProfile(string wallet, Model.Profile profile)
        {
            userProfile.SetProfile(wallet, profile);
        }
    }
}