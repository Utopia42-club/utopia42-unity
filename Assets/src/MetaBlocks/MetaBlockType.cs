using Newtonsoft.Json;

public class MetaBlockType : BlockType
{
    public readonly System.Type componentType;
    private readonly System.Type propertiesType;

    public MetaBlockType(byte id, string name, System.Type componentType, System.Type propertiesType)
        : base(id, name, false, 0, 0, 0, 0, 0, 0)
    {
        this.componentType = componentType;
        this.propertiesType = propertiesType;
    }

    public MetaBlock New(string props)
    {
        return new MetaBlock(this, (props == null || props.Length == 0) ? null :
            JsonConvert.DeserializeObject(props, propertiesType));
    }
}
