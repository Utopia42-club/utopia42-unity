using System.Numerics;

[System.Serializable]
public class Land
{
    public long x1;
    public long y1;
    public long x2;
    public long y2;
    public string ipfsKey;
    public long time;

    public override int GetHashCode()
    {
        return new Vector4(x1, y1, x2, y2).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        var land = (Land)obj;
        return x1 == land.x1 && x2 == land.x2 && y1 == land.y1 && y2 == land.y2;
    }
}
