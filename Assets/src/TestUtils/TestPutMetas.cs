using UnityEngine;

namespace src.TestUtils
{
    public class TestPutMetas : MonoBehaviour
    {
        [SerializeField] private string putMetasSampleName;

        private void Update()
        {
            if (putMetasSampleName == null || !Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            var textAsset = Resources.Load<TextAsset>("Test/" + putMetasSampleName);
            if (textAsset == null)
            {
                Debug.LogError("Could not load put metas sample");
                return;
            }

            UtopiaApi.INSTANCE.PlaceMetaBlocks(textAsset.text);
        }
    }
}