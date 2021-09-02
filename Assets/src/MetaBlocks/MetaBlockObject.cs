using UnityEngine;
public abstract class MetaBlockObject : MonoBehaviour
{
    private MetaBlock block;
    private Chunk chunk;
    private GameObject iconObject;

    public void Initialize(MetaBlock block, Chunk chunk)
    {
        this.block = block;
        this.chunk = chunk;
        DoInitialize();
    }

    protected abstract void DoInitialize();

    public abstract void OnDataUpdate();

    public abstract void Focus();

    public abstract void UnFocus();

    private void OnDestroy()
    {
        UnFocus();
        block.OnObjectDestroyed();
    }

    protected MetaBlock GetBlock()
    {
        return block;
    }
    protected Chunk GetChunk()
    {
        return chunk;
    }
    protected void CreateIcon()
    {
        iconObject = Instantiate(Resources.Load("MetaBlocks/MetaBlock") as GameObject, transform);
        var renderers = iconObject.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.material.mainTexture = block.type.GetIcon().texture;
    }

    protected GameObject GetIconObject()
    {
        return iconObject;
    }
}
