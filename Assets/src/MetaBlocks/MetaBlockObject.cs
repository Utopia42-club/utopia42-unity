using System;
using System.Collections.Generic;
using src.Model;
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
        protected bool ready = false;
        protected Land land;
        protected bool canEdit;
        protected State state;
        public readonly UnityEvent<State> stateChange = new UnityEvent<State>();

        protected virtual void Start()
        {
            canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land);
            ready = true;
            stateChange.AddListener(state => OnStateChanged(state));
        }

        public void Initialize(MetaBlock block, Chunk chunk)
        {
            this.block = block;
            this.chunk = chunk;
            DoInitialize();
        }

        public abstract bool IsReady();

        protected virtual void DoInitialize()
        {
            Start(); // Needs to be called manually to be executed before DoInitialize
        }

        public abstract void OnDataUpdate();

        public abstract void Focus();

        public abstract void UnFocus();

        protected abstract void OnStateChanged(State state); // TODO [detach metablock]: refactor?

        protected abstract List<string> GetSnackLines();

        protected void OnDestroy()
        {
            UnFocus();
            if (iconObject != null)
                Destroy(iconObject);
            block?.OnObjectDestroyed();
            stateChange.RemoveAllListeners();
        }

        protected MetaBlock GetBlock()
        {
            return block;
        }

        protected Chunk GetChunk()
        {
            return chunk;
        }

        protected GameObject GetIconObject()
        {
            return iconObject;
        }

        protected bool InLand(BoxCollider bc) //FIXME rename
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

        public abstract GameObject CreateSelectHighlight(Transform parent, bool show = true); // TODO [detach metablock] ?

        protected internal void UpdateState(State state)
        {
            this.state = state;
            stateChange.Invoke(state);
        }

        public abstract void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform,
            Vector3Int localPos, Action<GameObject> onLoad);
    }
}