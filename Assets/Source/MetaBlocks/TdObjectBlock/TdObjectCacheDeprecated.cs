using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.MetaBlocks.TdObjectBlock
{
    public static class TdObjectCacheDeprecated
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

            CloneMeshesAndTextures(clone);

            Assets[id].Add(tdObject);

            return clone;
        }

        private static void CloneMeshesAndTextures(GameObject go)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                var mats = renderer.materials;
                for (var i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat.Equals(World.INSTANCE.SelectedBlock) || mat.Equals(World.INSTANCE.HighlightBlock))
                    {
                        Object.Destroy(mat.mainTexture);
                        Object.Destroy(mat);
                    }
                    else
                    {
                        renderer.sharedMaterials[i] = mat;
                        if (mat.mainTexture != null)
                            renderer.sharedMaterials[i].mainTexture = Object.Instantiate(mat.mainTexture);
                    }
                }
            }

            foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
                meshFilter.sharedMesh = meshFilter.mesh;
        }

        private static TdObjectBlockObject FirstNonNull(List<TdObjectBlockObject> assets)
        {
            return assets?.FirstOrDefault(o => o != null && o.Obj != null);
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