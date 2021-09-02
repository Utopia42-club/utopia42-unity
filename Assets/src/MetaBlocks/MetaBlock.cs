using UnityEngine;

public class MetaBlock
{
    private MetaBlockObject blockObject;
    public readonly MetaBlockType type;
    private object properties;

    public MetaBlock(MetaBlockType type, object properties)
    {
        this.type = type;
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

    public void Focus()
    {
        if (blockObject != null) blockObject.Focus();
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
        if (blockObject != null) blockObject.OnDataUpdate();
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

