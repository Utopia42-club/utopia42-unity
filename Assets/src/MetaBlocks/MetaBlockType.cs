using System;
using Newtonsoft.Json;
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

        public abstract GameObject CreatePlaceHolder();

        public GameObject GetPlaceHolder()
        {
            if (placeHolder != null) return placeHolder;
            return placeHolder = CreatePlaceHolder();
        }
    }
}