using src.Utils;
using UnityEngine;

namespace src.MetaBlocks
{
    public abstract class MetaBlockObject : MonoBehaviour
    {
        private MetaBlock block;
        private Chunk chunk;
        private GameObject iconObject;

        public void Initialize(MetaBlock block, Chunk chunk)
        {
            this.block = block;
            this.chunk = chunk;
            DoInitialize();
        }

        public abstract bool IsReady();

        protected abstract void DoInitialize();

        public abstract void OnDataUpdate();

        public abstract void Focus(Voxels.Face face);

        public abstract void UnFocus();

        private void OnDestroy()
        {
            UnFocus();
            block.OnObjectDestroyed();
        }

        protected MetaBlock GetBlock()
        {
            return block;
        }
        protected Chunk GetChunk()
        {
            return chunk;
        }

        protected void CreateIcon(bool failed = false)
        {
            if (iconObject != null) DestroyImmediate(iconObject);
            iconObject = Instantiate(Resources.Load("MetaBlocks/MetaBlock") as GameObject, transform);
            var renderers = iconObject.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.material.mainTexture = block.type.GetIcon(failed).texture;
        }

        protected GameObject GetIconObject()
        {
            return iconObject;
        }
        
        protected bool IsInLand(BoxCollider bc)
        {
            var transform = bc.transform;
            
            var center = bc.center;
            var size = bc.size;
            
            var min = center - size * 0.5f;
            var max = center + size * 0.5f;

            return
                IsInLand(transform.TransformPoint(new Vector3(min.x, min.y, min.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(min.x, min.y, max.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(min.x, max.y, min.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(min.x, max.y, max.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(max.x, min.y, min.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(max.x, min.y, max.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(max.x, max.y, min.z))) &&
                IsInLand(transform.TransformPoint(new Vector3(max.x, max.y, max.z)));
        }
        
        protected bool IsInLand(MeshRenderer meshRenderer)
        {
            var bounds = meshRenderer.bounds;
            var center = bounds.center;
            var size = bounds.size;
            
            var min = center - size * 0.5f;
            var max = center + size * 0.5f;

            return
                IsInLand(new Vector3(min.x, min.y, min.z)) &&
                IsInLand(new Vector3(min.x, min.y, max.z)) &&
                IsInLand(new Vector3(min.x, max.y, min.z)) &&
                IsInLand(new Vector3(min.x, max.y, max.z)) &&
                IsInLand(new Vector3(max.x, min.y, min.z)) &&
                IsInLand(new Vector3(max.x, min.y, max.z)) &&
                IsInLand(new Vector3(max.x, max.y, min.z)) &&
                IsInLand(new Vector3(max.x, max.y, max.z));
        }
        private bool IsInLand(Vector3 p)
        {
            return p.x >= block.land.x1 && p.x <= block.land.x2 && p.z >= block.land.y1 && p.z <= block.land.y2;
        }
    }
}
