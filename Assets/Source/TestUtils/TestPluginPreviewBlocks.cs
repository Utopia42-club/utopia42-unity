using Source.Utils;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestPluginPreviewBlocks : MonoBehaviour
    {
        [SerializeField] private string jsonSample = "voxPutBlocksSampleRequest";

        private void Update()
        {
            if (jsonSample == null || !Input.GetKeyDown(KeyCode.T) ||
                (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))) return;
            var textAsset = Resources.Load<TextAsset>("Test/" + jsonSample);
            if (textAsset == null)
            {
                Debug.LogError("Could not load json sample");
                return;
            }

            UtopiaApi.INSTANCE.PreviewBlocksWithOffset(textAsset.text,
                Vectors.FloorToInt(Player.INSTANCE.CamPosition) + 5 * Vector3Int.up);
        }
    }
}