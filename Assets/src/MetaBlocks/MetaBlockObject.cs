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

        protected bool InLand(BoxCollider bc)
        {
            if (block.land == null)
                return true;

            var bcTransform = bc.transform;

            var center = bc.center;
            var size = bc.size;

            var min = center - size * 0.5f;
            var max = center + size * 0.5f;

            return
                InLand(bcTransform.TransformPoint(new Vector3(min.x, min.y, min.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(min.x, min.y, max.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(min.x, max.y, min.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(min.x, max.y, max.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(max.x, min.y, min.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(max.x, min.y, max.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(max.x, max.y, min.z))) &&
                InLand(bcTransform.TransformPoint(new Vector3(max.x, max.y, max.z)));
        }

        protected bool InLand(MeshRenderer meshRenderer)
        {
            if (block.land == null)
                return true;

            var bounds = meshRenderer.bounds;
            var center = bounds.center;
            var size = bounds.size;

            var min = center - size * 0.5f;
            var max = center + size * 0.5f;

            return
                InLand(new Vector3(min.x, min.y, min.z)) &&
                InLand(new Vector3(min.x, min.y, max.z)) &&
                InLand(new Vector3(min.x, max.y, min.z)) &&
                InLand(new Vector3(min.x, max.y, max.z)) &&
                InLand(new Vector3(max.x, min.y, min.z)) &&
                InLand(new Vector3(max.x, min.y, max.z)) &&
                InLand(new Vector3(max.x, max.y, min.z)) &&
                InLand(new Vector3(max.x, max.y, max.z));
        }

        private bool InLand(Vector3 p)
        {
            if (block.land == null)
                return true;
            return block.land.Contains(p);
        }
    }
}