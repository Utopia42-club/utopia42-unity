using System;
using Newtonsoft.Json;
using src.Model;

namespace src.MetaBlocks
{
    public class MetaBlockType : BlockType
    {
        public readonly Type componentType;
        public readonly bool inMemory;
        private readonly Type propertiesType;

        public MetaBlockType(byte id, string name, Type componentType, Type propertiesType, bool inMemory = false)
            : base(id, name, false, 0, 0, 0, 0, 0, 0)
        {
            this.componentType = componentType;
            this.inMemory = inMemory;
            this.propertiesType = propertiesType;
        }

        public MetaBlock New(Land land, string props)
        {
            return new MetaBlock(this, land,
                (props == null || props.Length == 0) ? null : DeserializeProps(props));
        }

        public object DeserializeProps(string props)
        {
            return JsonConvert.DeserializeObject(props, propertiesType);
        }
    }
}