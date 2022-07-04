using System.Collections.Generic;
using ReadyPlayerMe;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestAvatarImport : MonoBehaviour
    {
        private readonly AvatarLoader avatarLoader = new() {UseAvatarCaching = true};
        private int avatarIndex = -1;

        private static readonly List<string> Urls = new()
        {
            "https://d1a370nemizbjq.cloudfront.net/efaddf31-5b4f-4a5e-954a-741728492150.glb",
            "https://d1a370nemizbjq.cloudfront.net/d640df44-d1ff-449b-a9f2-10989794bd86.glb",
            "https://d1a370nemizbjq.cloudfront.net/a6567559-7fd1-4a3b-bee6-40b9a7b8e76b.glb",
            "https://d1a370nemizbjq.cloudfront.net/cd07bc8d-941f-4c94-b0d7-7fbdf0c4f126.glb"
        };

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            Debug.Log($"Started loading avatar. [{Time.timeSinceLevelLoad:F2}]");

            avatarLoader.OnCompleted += (sender, args) =>
            {
                args.Avatar.transform.position = Player.INSTANCE.GetPosition() + 3 * Vector3.up;
                Debug.Log($"Loaded avatar.");
            };
            avatarLoader.OnFailed += (sender, args) => { Debug.Log(args.Type); };

            avatarIndex += 1;
            if (avatarIndex > 3) avatarIndex = 0;
            avatarLoader.LoadAvatar(Urls[avatarIndex]);
        }
    }
}