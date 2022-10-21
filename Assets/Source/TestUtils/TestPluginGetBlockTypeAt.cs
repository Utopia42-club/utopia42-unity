using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Source.TestUtils
{
    public class TestPluginGetBlockTypeAt : MonoBehaviour
    {
        [SerializeField] private Vector3Int typePosition;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) ||
                !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) return;
            var type = UtopiaApi.INSTANCE.GetBlockTypeAt(JsonConvert.SerializeObject(typePosition));
            Debug.Log($"Block type at {typePosition} is {type}");
        }
    }
}