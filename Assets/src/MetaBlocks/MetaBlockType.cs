using System;
using Newtonsoft.Json;
using Siccity.GLTFUtility;
using src.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src.MetaBlocks
{
    public abstract class MetaBlockType : BlockType
    {
        public readonly Type componentType;
        public readonly bool inMemory;
        private readonly Type propertiesType;
        protected GameObject placeHolder;

        public MetaBlockType(byte id, string name, Type componentType, Type propertiesType, bool inMemory = false)
            : base(id, name, false, 0, 0, 0, 0, 0, 0)
        {
            this.componentType = componentType;
            this.inMemory = inMemory;
            this.propertiesType = propertiesType;
        }

        public MetaBlock Instantiate(Land land, string props)
        {
            return new MetaBlock(this, land,
                (props == null || props.Length == 0) ? null : DeserializeProps(props));
        }

        public object DeserializeProps(string props)
        {
            return JsonConvert.DeserializeObject(props, propertiesType);
        }

        public abstract GameObject CreatePlaceHolder(bool error, bool withCollider);

        public GameObject GetPlaceHolder()
        {
            if (placeHolder != null) return placeHolder;
            return placeHolder = CreatePlaceHolder(false, false);
        }

        public virtual MetaPosition GetPutPosition(Vector3 purePosition)
        {
            return new MetaPosition(purePosition);
        }
        
        protected static GameObject Create3dPlaceHolder(string glbName, string objName, bool withCollider, float localScale)
        {
            var go = Importer.LoadFromFile($"Assets/Resources/PlaceHolder/{glbName}.glb");
            if (withCollider)
                go.AddComponent<BoxCollider>();
            go.SetActive(false);
            go.name = "marker placeholder";
            go.transform.localScale = localScale * Vector3.one;
            return go;
        }
    }
}