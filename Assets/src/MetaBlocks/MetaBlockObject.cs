using System;
using System.Linq;
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
        public Chunk chunk;
        protected Land land;
        protected bool canEdit;
        public State State { get; protected set; }
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
            Block = block;
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
            CreateSelectHighlight(Transform parent, bool show = true);

        protected internal void UpdateState(State state)
        {
            State = state;
            stateChange.Invoke(state);
        }

        public void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform,
            MetaLocalPosition localPos,
            Action<GameObject> onLoad) // TODO [detach metablock]: will not work with link and marker metablocks (they only have empty state)
        {
            var goRef = gameObject;
            var gameObjectTransform = goRef.transform;
            gameObjectTransform.parent = World.INSTANCE.transform;
            gameObjectTransform.localPosition = highlightChunkTransform.transform.localPosition + localPos.position;
            Initialize(block, null);

            stateChange.AddListener((state) =>
            {
                if (goRef == null) return;
                if (state != State.Ok)
                {
                    if (state != State.Loading && state != State.LoadingMetadata)
                    {
                        Destroy(goRef);
                        goRef = null;
                    }

                    return;
                }

                var go = CreateSelectHighlight(highlightChunkTransform);
                if (go != null) onLoad(go);

                foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
                    renderer.enabled = false;
            });
        }

        public float? GetCenterY(out float? height)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return height = null;

            float
                minY = float.PositiveInfinity,
                maxY = float.NegativeInfinity;
            foreach (var child in renderers)
            {
                var bounds = child.bounds;
                var min = bounds.min;
                var max = bounds.max;

                if (min.y < minY) minY = min.y;
                if (max.y > maxY) maxY = max.y;
            }

            height = maxY - minY;
            return (minY + maxY) / 2;
        }

        public float? GetHeight()
        {
            GetCenterY(out var h);
            return h;
        }

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

        protected static void AdjustHighlightBox(Transform highlightBox, BoxCollider referenceCollider, bool active)
        {
            var colliderTransform = referenceCollider.transform;
            highlightBox.transform.rotation = colliderTransform.rotation;

            var size = referenceCollider.size;
            var minPos = referenceCollider.center - size / 2;

            var gameObjectTransform = referenceCollider.gameObject.transform;
            size.Scale(gameObjectTransform.localScale);
            size.Scale(gameObjectTransform.parent.localScale);

            highlightBox.localScale = size;
            highlightBox.position = colliderTransform.TransformPoint(minPos);
            highlightBox.gameObject.SetActive(active);
        }

        protected static void DeepDestroy3DObject(GameObject go, bool immediate = true)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;
                if (immediate)
                {
                    DestroyImmediate(mat.mainTexture);
                    if (!mat.Equals(World.INSTANCE.SelectedBlock) && !mat.Equals(World.INSTANCE.HighlightBlock))
                        DestroyImmediate(mat);
                }
                else
                {
                    Destroy(mat.mainTexture);
                    if (!mat.Equals(World.INSTANCE.SelectedBlock) && !mat.Equals(World.INSTANCE.HighlightBlock))
                        Destroy(mat);
                }
            }

            foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
            {
                if (immediate)
                    DestroyImmediate(meshFilter.sharedMesh);
                else
                    Destroy(meshFilter.sharedMesh);
            }

            if (immediate)
                DestroyImmediate(go.gameObject);
            else
                Destroy(go.gameObject);
        }
    }
}