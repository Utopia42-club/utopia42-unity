using src.Utils;
using UnityEngine;

namespace src.TestUtils
{
    public class TestPutMetas : MonoBehaviour
    {
        [SerializeField] private string putMetasSampleName;

        private void Update()
        {
            if (putMetasSampleName == null || !Input.GetKeyDown(KeyCode.T) ||
                (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))) return;
            var textAsset = Resources.Load<TextAsset>("Test/" + putMetasSampleName);
            if (textAsset == null)
            {
                Debug.LogError("Could not load put metas sample");
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift))
                UtopiaApi.INSTANCE.PreviewMetaBlocksWithOffset(textAsset.text,
                    Vectors.FloorToInt(Player.INSTANCE.transform.position) + 5 * Vector3Int.up);
            else
                UtopiaApi.INSTANCE.PlaceMetaBlocksWithOffset(textAsset.text,
                    Vectors.FloorToInt(Player.INSTANCE.transform.position) + 5 * Vector3Int.up);
        }
    }
}