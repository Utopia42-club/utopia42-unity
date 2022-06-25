using ReadyPlayerMe;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestAvatarImport : MonoBehaviour
    {
        // [SerializeField]
        // private string avatarUrl = "https://d1a370nemizbjq.cloudfront.net/209a1bc2-efed-46c5-9dfd-edc8a1d9cbe4.glb";
        private string avatarUrl = "https://d1a370nemizbjq.cloudfront.net/1b5c7df0-b903-4540-9643-e7db9755bf55.glb";

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            Debug.Log($"Started loading avatar. [{Time.timeSinceLevelLoad:F2}]");

            var avatarLoader = new AvatarLoader();
            avatarLoader.OnCompleted += (sender, args) =>
            {
                args.Avatar.transform.position = Player.INSTANCE.GetPosition() + 5 * Vector3.up;
                Debug.Log($"Loaded avatar. [{Time.timeSinceLevelLoad:F2}]");
            };
            avatarLoader.OnFailed += (sender, args) => { Debug.Log(args.Type); };

            avatarLoader.LoadAvatar(avatarUrl);
        }
    }
}