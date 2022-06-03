using System;
using System.Collections.Generic;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace src.MetaBlocks
{
    public abstract class MetaBlockObject : MonoBehaviour
    {
        private MetaBlock block;
        protected Chunk chunk;
        private GameObject iconObject;
        public readonly UnityEvent<StateMsg> stateChange = new UnityEvent<StateMsg>();

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

        public abstract void UpdateStateAndView(StateMsg msg, Voxels.Face face); // TODO [detach metablock]: refactor?

        protected abstract List<string> GetFaceSnackLines(Voxels.Face face);
        
        protected void OnDestroy()
        {
            UnFocus();
            if (iconObject != null)
                Destroy(iconObject);
            block?.OnObjectDestroyed();
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
            return; // TODO [detach metablock]: remove/change function?
            if (chunk == null) return;
            if (iconObject != null)
                Destroy(iconObject);
            iconObject = Instantiate(Resources.Load("MetaBlocks/MetaBlock") as GameObject, transform);
            foreach (var r in iconObject.GetComponentsInChildren<Renderer>())
                r.material.mainTexture = block.type.GetIcon(failed).texture;
        }

        protected GameObject GetIconObject()
        {
            return iconObject;
        }

        protected bool InLand(BoxCollider bc)//FIXME rename
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

        public abstract void ShowFocusHighlight();

        public abstract void RemoveFocusHighlight();

        public abstract GameObject CreateSelectHighlight(Transform parent, bool show = true);

        protected abstract void UpdateState(StateMsg stateMsg);

        public abstract void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform, Vector3Int localPos, Action<GameObject> onLoad);
    }
}