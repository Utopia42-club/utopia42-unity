using System;
using Source.Model;
using Source.Service;
using Source.Utils;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Source.MetaBlocks
{
    public class MetaBlock
    {
        public static readonly MetaBlock DELETED_METABLOCK = new MetaBlock(null, null, null);

        public MetaBlockObject blockObject { private set; get; }
        public readonly Land land;
        public readonly MetaBlockType type;
        private object properties;
        public bool IsCursor { private set; get; } = false;

        public bool IsActive => blockObject != null && blockObject.gameObject.activeSelf;

        public MetaBlock(MetaBlockType type, Land land, object properties, bool isCursor = false)
        {
            this.type = type;
            this.land = land;
            this.properties = properties;
            this.IsCursor = isCursor;
        }

        public void RenderAt(Transform parent, Vector3 position, Chunk chunk)
        {
            if (blockObject != null) throw new Exception("Already rendered.");
            GameObject go = new GameObject("MetaBlock");
            blockObject = (MetaBlockObject) go.AddComponent(type.componentType);
            go.transform.parent = parent;
            go.transform.localPosition = position;
            blockObject.Initialize(this, chunk);
        }

        internal void OnObjectDestroyed()
        {
            blockObject = null;
        }

        public void SetProps(object props, Land land)
        {
            if (Equals(properties, props)) return;
            properties = props;
            if (land != null)
                WorldService.INSTANCE.MarkLandChanged(land);
            if (blockObject != null)
                blockObject.OnDataUpdate();
        }

        public object GetProps()
        {
            return properties;
        }

        public void DestroyView(bool immediate = true)
        {
            if (blockObject == null) return;
            if (immediate)
                Object.DestroyImmediate(blockObject.gameObject);
            else
                Object.Destroy(blockObject.gameObject);
        }

        public void SetActive(bool active)
        {
            if (blockObject != null)
                blockObject.gameObject.SetActive(active);
        }

        public static MetaBlock Parse(Land land, MetaBlockData meta)
        {
            var type = (MetaBlockType) Blocks.GetBlockType(meta.type);
            if (type == null) return null;
            try
            {
                return type.Instantiate(land, meta.properties);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception occured while parsing meta props. " + ex);
                return null;
            }
        }

        public void CreateSelectHighlight(Transform highlightChunkTransform, MetaLocalPosition localPos,
            Action<GameObject> onLoad, out Action cleanUpAction)
        {
            Action cleanUpActionA = null, cleanUpActionB = null;
            cleanUpAction = () =>
            {
                cleanUpActionA?.Invoke();
                cleanUpActionB?.Invoke();
            };

            if (blockObject != null)
            {
                var go = blockObject.CreateSelectHighlight(highlightChunkTransform);
                if (go != null)
                {
                    onLoad(go);

                    void CallBack(State state)
                    {
                        var go = blockObject.CreateSelectHighlight(highlightChunkTransform);
                        if (go != null) onLoad(go);
                    }

                    blockObject.stateChange.AddListener(CallBack);
                    cleanUpActionA = () =>
                    {
                        if(blockObject != null)
                            blockObject.stateChange.RemoveListener(CallBack);
                    };

                    return;
                }
            }

            var gameObject = new GameObject
            {
                name = "Temp game object"
            };
            blockObject = (MetaBlockObject) gameObject.AddComponent(type.componentType);
            blockObject.LoadSelectHighlight(this, highlightChunkTransform, localPos, onLoad);

            cleanUpActionB = () =>
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject);
            };
        }

        public void UpdateWorldPosition(Vector3 raycastHitPoint)
        {
            if (blockObject == null)
            {
                Debug.LogWarning(
                    "Cannot move uninitialized metablock");
                return;
            }

            if (blockObject.chunk != null || land != null)
            {
                Debug.LogWarning(
                    "Can not update the world position of a metablock that already belongs to a land/chunk");
                return;
            }

            var height = blockObject.GetHeight();
            blockObject.gameObject.transform.position =
                raycastHitPoint + (height.HasValue ? height.Value / 2 : 0) * Vector3.up;
        }
    }
}