using System;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace src.MetaBlocks
{
    public abstract class MetaBlockObject : MonoBehaviour
    {
        public MetaBlock Block { get; private set; }
        public bool Started { get; private set; } = false;
        protected Chunk chunk;
        protected Land land;
        protected bool canEdit;
        protected State state;
        protected SnackItem snackItem;
        public readonly UnityEvent<State> stateChange = new UnityEvent<State>();

        protected void Start()
        {
            canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land);
            stateChange.AddListener(OnStateChanged);
            gameObject.name = Block.type.name + " block object";
            Started = true;
        }

        public void Initialize(MetaBlock block, Chunk chunk)
        {
            this.Block = block;
            this.chunk = chunk;
            Start();
            DoInitialize();
        }

        public void Focus()
        {
            SetupDefaultSnack();
            if (!canEdit) return;
            ShowFocusHighlight();
        }

        public void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            RemoveFocusHighlight();
        }

        protected abstract void OnStateChanged(State state);

        protected Chunk GetChunk()
        {
            return chunk;
        }

        protected bool InLand(BoxCollider bc) //FIXME rename
        {
            if (Block.land == null)
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
            if (Block.land == null)
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
            if (Block.land == null)
                return true;
            return Block.land.Contains(p);
        }

        public abstract GameObject
            CreateSelectHighlight(Transform parent, bool show = true); // TODO [detach metablock] ?

        protected internal void UpdateState(State state)
        {
            this.state = state;
            stateChange.Invoke(state);
        }

        public abstract void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform,
            MetaLocalPosition localPos, Action<GameObject> onLoad);

        protected abstract void DoInitialize();

        public abstract void OnDataUpdate();

        public abstract void SetToMovingState();
        public abstract void ExitMovingState();

        protected abstract void SetupDefaultSnack();

        public abstract void ShowFocusHighlight();

        public abstract void RemoveFocusHighlight();

        protected virtual void OnDestroy()
        {
            UnFocus();
            Block?.OnObjectDestroyed();
            stateChange.RemoveAllListeners();
        }
    }
}