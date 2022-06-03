using Siccity.GLTFUtility;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockType : MetaBlockType
    {
        public const string Name = "3d_object";

        public TdObjectBlockType(byte id) : base(id, Name, typeof(TdObjectBlockObject), typeof(TdObjectBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder()
        {
            var go = Importer.LoadFromFile("Assets/Resources/PlaceHolder/3d_object.glb");
            // TODO [detach metablock]: adjust dimensions and remove collider go from it

            var colliderTransform = TdObjectBlockObject.GetMeshColliderTransform(go);
            if (colliderTransform == null)
                go.AddComponent<BoxCollider>();
            else
                TdObjectBlockObject.PrepareMeshCollider(colliderTransform);
            go.SetActive(false);
            go.name = "3d_object placeholder";
            return go;
        }
    }
}