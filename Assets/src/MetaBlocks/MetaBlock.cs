using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks
{
    public class MetaBlock
    {
        private MetaBlockObject blockObject;
        public readonly Land land;
        public readonly MetaBlockType type;
        private object properties;

        public MetaBlock(MetaBlockType type, Land land, object properties)
        {
            this.type = type;
            this.land = land;
            this.properties = properties;
        }

        public void RenderAt(Transform parent, Vector3Int position, Chunk chunk)
        {
            if (blockObject != null) throw new System.Exception("Already rendered.");
            GameObject go = new GameObject("Image");
            blockObject = (MetaBlockObject)go.AddComponent(type.componentType);
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

        public bool Focus(Voxels.Face face)
        {
            if (blockObject != null && blockObject.IsReady())
            {
                blockObject.Focus(face);
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
            VoxelService.INSTANCE.MarkLandChanged(land);
            if (blockObject != null) 
                blockObject.OnDataUpdate();
        }

        public object GetProps()
        {
            return properties;
        }

        public void Destroy()
        {
            if (blockObject != null)
                Object.DestroyImmediate(blockObject.gameObject);
        }
    }
}

