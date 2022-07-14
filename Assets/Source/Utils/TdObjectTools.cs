using System.Linq;
using UnityEngine;

namespace Source.Utils
{
    public static class TdObjectTools
    {
        public static Vector3 GetRendererCenter(GameObject go)
        {
            float
                minX = float.PositiveInfinity,
                maxX = float.NegativeInfinity,
                minY = float.PositiveInfinity,
                maxY = float.NegativeInfinity,
                minZ = float.PositiveInfinity,
                maxZ = float.NegativeInfinity;

            foreach (var child in go.GetComponentsInChildren<Renderer>())
            {
                var bounds = child.bounds;
                var min = bounds.min;
                var max = bounds.max;

                if (min.x < minX) minX = min.x;
                if (min.y < minY) minY = min.y;
                if (min.z < minZ) minZ = min.z;

                if (max.x > maxX) maxX = max.x;
                if (max.y > maxY) maxY = max.y;
                if (max.z > maxZ) maxZ = max.z;
            }

            return new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
        }

        public static Vector3 GetRendererSize(Vector3 center, GameObject go)
        {
            var bounds = new Bounds(center, Vector3.zero);
            foreach (var child in go.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(child.bounds);
            }

            return bounds.size;
        }

        public static Transform GetMeshColliderTransform(GameObject go)
        {
            return go.GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t.name.EndsWith("_collider"));
        }

        public static MeshCollider PrepareMeshCollider(Transform colliderTransform)
        {
            colliderTransform.localScale = 1.01f * colliderTransform.localScale;
            var colliderRenderer = colliderTransform.gameObject.GetComponent<MeshRenderer>();
            colliderRenderer.enabled = false;
            colliderRenderer.material = World.INSTANCE.HighlightBlock;

            return colliderTransform.gameObject.AddComponent<MeshCollider>();
        }
    }
}