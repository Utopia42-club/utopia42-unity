using ReadyPlayerMe;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestAvatarImport : MonoBehaviour
    {
        [SerializeField] private bool gameoObjectClone = false;
        
        private GameObject loadedAvatar;
        private readonly AvatarLoader avatarLoader = new(){UseAvatarCaching = true};
        
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            Debug.Log($"Started loading avatar. [{Time.timeSinceLevelLoad:F2}]");

            if (gameoObjectClone && loadedAvatar != null)
            {
                var clone = Instantiate(loadedAvatar);
                clone.transform.position = Player.INSTANCE.GetPosition() + 3 * Vector3.up;
                Debug.Log($"Cloned avatar.");
                return;
            }
            
            avatarLoader.OnCompleted += (sender, args) =>
            {
                loadedAvatar = args.Avatar; 
                loadedAvatar.transform.position = Player.INSTANCE.GetPosition() + 3 * Vector3.up;
                Debug.Log($"Loaded avatar.");
            };
            avatarLoader.OnFailed += (sender, args) => { Debug.Log(args.Type); };
            
            avatarLoader.LoadAvatar(AvatarController.DefaultAvatarUrl);
            // avatarLoader.ImportModel(Resources.Load<TextAsset>($"Avatars/default").bytes);
        }
    }
}