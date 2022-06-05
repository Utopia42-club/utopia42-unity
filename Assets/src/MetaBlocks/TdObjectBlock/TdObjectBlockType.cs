using Siccity.GLTFUtility;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockType : MetaBlockType
    {
        public TdObjectBlockType(byte id) : base(id, "3d_object", typeof(TdObjectBlockObject),
            typeof(TdObjectBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error)
        {
            var go = Importer.LoadFromFile($"Assets/Resources/PlaceHolder/" +
                                           (!error ? "3d_object.glb" : "3d_object_error.glb"));
            var colliderTransform = TdObjectBlockObject.GetMeshColliderTransform(go);
            if (colliderTransform == null)
                go.AddComponent<BoxCollider>();
            else
                TdObjectBlockObject.PrepareMeshCollider(colliderTransform);
            go.SetActive(false);
            go.name = "3d object placeholder";
            go.transform.localScale = 2 * Vector3.one; 
            return go;
        }
    }
}