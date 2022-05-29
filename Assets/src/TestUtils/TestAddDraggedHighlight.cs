using src.Model;
using UnityEngine;

namespace src.TestUtils
{
    public class TestAddDraggedHighlight : MonoBehaviour
    {
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.T) || !Input.GetKey(KeyCode.LeftShift)) return;
            BlockSelectionController.INSTANCE.AddDraggedGlbHighlight("https://dweb.link/ipfs/Qmeix4Fqxyqy7XzNVfwJnS2txjLJDer1a6Ad722LjZxwbw");
        }
    }
}