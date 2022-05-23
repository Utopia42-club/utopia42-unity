using src.Model;
using UnityEngine;

namespace src.TestUtils
{
    public class TestAddHighlight : MonoBehaviour
    {
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            BlockSelectionController.INSTANCE.AddHighlight(new VoxelPosition(new Vector3Int(-177, 1, -71), new Vector3Int(9, 3, 11)));
        }
    }
}