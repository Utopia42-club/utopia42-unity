using Source.Utils;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestPluginPutBlocks : MonoBehaviour
    {
        [SerializeField] private string putBlocksSampleName = "voxPutBlocksSampleRequest";

        private void Update()
        {
            if (putBlocksSampleName == null || !Input.GetKeyDown(KeyCode.T) ||
                (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))) return;
            var textAsset = Resources.Load<TextAsset>("Test/" + putBlocksSampleName);
            if (textAsset == null)
            {
                Debug.LogError("Could not load json sample");
                return;
            }

            UtopiaApi.INSTANCE.PlaceBlocksWithOffset(textAsset.text,
                Vectors.FloorToInt(Player.INSTANCE.GetPosition()) + 5 * Vector3Int.up);
        }
    }
}