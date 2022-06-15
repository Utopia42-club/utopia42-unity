using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src.MetaBlocks.TdObjectBlock
{
    public static class TdObjectCache
    {
        private static readonly Dictionary<Key, List<TdObjectBlockObject>> Assets = new();

        public static GameObject GetAsset(TdObjectBlockObject tdObject)
        {
            var props = (TdObjectBlockProperties) tdObject.Block.GetProps();
            if (props == null)
            {
                Debug.LogError("3D object properties is null");
                return null;
            }

            var id = new Key(props.url, props.type);
            if (!Assets.TryGetValue(id, out var assets)) return null;

            var asset = FirstNonNull(assets);
            if (asset == null)
            {
                Assets.Remove(id);
                return null;
            }

            var clone = Object.Instantiate(asset.Obj);
            foreach (var collider in clone.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(collider);
            foreach (var focusable in clone.GetComponentsInChildren<Focusable>())
                Object.DestroyImmediate(focusable);

            asset.hasClone = true;
            Assets[id].Add(tdObject);

            return clone;
        }

        private static TdObjectBlockObject FirstNonNull(List<TdObjectBlockObject> assets)
        {
            return assets?.FirstOrDefault(o => o != null && o.Obj != null);
        }

        public static bool HasClone(TdObjectBlockObject tdObject)
        {
            if (!tdObject.hasClone) return false;
            var props = (TdObjectBlockProperties) tdObject.Block.GetProps();
            if (props == null) return false;
            var id = new Key(props.url, props.type);
            if (!Assets.TryGetValue(id, out var assets)) return false;
            return FirstNonNull(assets) != null;
        }

        public static void Add(TdObjectBlockObject tdObject)
        {
            var props = (TdObjectBlockProperties) tdObject.Block.GetProps();
            if (props == null)
            {
                Debug.LogError("3D object properties is null");
                return;
            }

            var id = new Key(props.url, props.type);
            if (Assets.TryGetValue(id, out var asset))
            {
                if (asset != null)
                {
                    // Debug.LogWarning("3D object cache already contains an entry with key " + id);
                    return;
                }
            }

            Assets[id] = new List<TdObjectBlockObject>() {tdObject};
        }

        private class Key
        {
            private readonly string url;
            private readonly TdObjectBlockProperties.TdObjectType type;

            public Key(string url, TdObjectBlockProperties.TdObjectType type)
            {
                this.url = url;
                this.type = type;
            }

            private bool Equals(Key other)
            {
                return url == other.url && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Key) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(url, (int) type);
            }
        }
    }
}