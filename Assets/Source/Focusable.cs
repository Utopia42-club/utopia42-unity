using UnityEngine;

namespace Source
{
    public abstract class Focusable : MonoBehaviour
    {
        protected bool initialized = false;
        public abstract void Focus(Vector3? point = null);

        public abstract void UnFocus();
        
        public abstract Vector3? GetBlockPosition();
    }
}