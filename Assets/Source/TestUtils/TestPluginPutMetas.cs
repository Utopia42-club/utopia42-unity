using Source.Utils;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestPluginPutMetas : MonoBehaviour
    {
        [SerializeField] private string putMetasSampleName = "putTdObjSampleRequest";

        private void Update()
        {
            if (putMetasSampleName == null || !Input.GetKeyDown(KeyCode.T) ||
                (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))) return;
            var textAsset = Resources.Load<TextAsset>("Test/" + putMetasSampleName);
            if (textAsset == null)
            {
                Debug.LogError("Could not load json sample");
                return;
            }

            UtopiaApi.INSTANCE.PlaceMetaBlocksWithOffset(textAsset.text,
                Vectors.FloorToInt(Player.INSTANCE.CamPosition) + 5 * Vector3Int.up);
        }
    }
}