using System;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src.MetaBlocks
{
    public class MetaBlock
    {
        public static readonly MetaBlock DELETED_METABLOCK = new MetaBlock(null, null, null);

        public MetaBlockObject blockObject { private set; get; }
        public readonly Land land;
        public readonly MetaBlockType type;
        private object properties;

        public MetaBlock(MetaBlockType type, Land land, object properties)
        {
            this.type = type;
            this.land = land;
            this.properties = properties;
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

        public bool IsPositioned()
        {
            return blockObject != null;
        }

        public Vector3 GetPosition()
        {
            return blockObject.transform.position;
        }

        public bool Focus() 
        {
            if (blockObject != null && blockObject.IsReady())
            {
                blockObject.Focus();
                return true;
            }

            return false;
        }

        public void UnFocus()
        {
            if (blockObject != null) blockObject.UnFocus();
        }

        internal void OnObjectDestroyed()
        {
            blockObject = null;
        }

        public void SetProps(object props, Land land)
        {
            if (Equals(properties, props)) return;
            properties = props;
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

        public void CreateSelectHighlight(Transform highlightChunkTransform, Vector3Int localPos,
            Action<GameObject> onLoad, out GameObject referenceGo) // TODO [detach metablock] ?
        {
            referenceGo = null;
            if (blockObject != null)
            {
                var go = blockObject.CreateSelectHighlight(highlightChunkTransform);
                if (go != null)
                {
                    onLoad(go);
                    return;
                }

                blockObject.stateChange.AddListener(state =>
                {
                    if (state != State.Ok) return;
                    var go = blockObject.CreateSelectHighlight(highlightChunkTransform);
                    if (go != null) onLoad(go);
                });
                return;
            }

            var gameObject = referenceGo = new GameObject
            {
                name = "Temp game object"
            };
            blockObject = (MetaBlockObject) gameObject.AddComponent(type.componentType);
            blockObject.LoadSelectHighlight(this, highlightChunkTransform, localPos, onLoad);
        }
    }
}