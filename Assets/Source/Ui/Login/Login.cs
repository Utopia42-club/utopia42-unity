using Source.Service.Auth;
using Source.Service.Ethereum;
using Source.Ui.Snack;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Login
{
    public class Login : MonoBehaviour
    {
        private GameObject panel;
        private Button submitButton;
        private Button guestButton;

        private Vector3? startingPosition = null;
        private VisualElement root;
        private VisualElement memberTile;
        private VisualElement guestTile;
        private Button guestTabButton;
        private Button memberTabButton;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            memberTile = root.Q<VisualElement>("memberTile");
            guestTile = root.Q<VisualElement>("guestTile");

            guestButton = root.Q<Button>("guestButton");
            guestButton.clickable.clicked += () => AuthService.Instance.SetUpGuestSession();

            submitButton = root.Q<Button>("enterButton");
            submitButton.clickable.clicked += () => AuthService.Instance.Login();

            memberTabButton = root.Q<Button>("memberTabButton");
            memberTabButton.clickable.clicked += () => SelectTab(0);
            guestTabButton = root.Q<Button>("guestTabButton");
            guestTabButton.clickable.clicked += () => SelectTab(1);

            SelectTab(0);
        }

        private void SelectTab(int index)
        {
            switch (index)
            {
                case 0:
                    guestTile.style.display = DisplayStyle.None;
                    memberTile.style.display = DisplayStyle.Flex;
                    guestTabButton.RemoveFromClassList("selected-login-tab-button");
                    memberTabButton.AddToClassList("selected-login-tab-button");
                    break;
                case 1:
                    guestTile.style.display = DisplayStyle.Flex;
                    memberTile.style.display = DisplayStyle.None;
                    guestTabButton.AddToClassList("selected-login-tab-button");
                    memberTabButton.RemoveFromClassList("selected-login-tab-button");
                    break;
            }
        }
    }
}