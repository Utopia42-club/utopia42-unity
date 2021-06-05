using System.Numerics;
using UnityEngine;

public class Land
{
    public readonly Vector2Int from;
    public readonly Vector2Int to;
    public readonly BigInteger time;
    public readonly string hash;

    public Land(Vector2Int from, Vector2Int to, BigInteger time, string hash)
    {
        this.from = from;
        this.to = to;
        this.time = time;
        this.hash = hash;
    }

    public Land(int x1, int y1, int x2, int y2, BigInteger time, string hash)
        : this(new Vector2Int(x1, y1), new Vector2Int(x2, y2), time, hash)
    {
    }

    public override string ToString()
    {
        return string.Format("from: %s, to: %s, time: %s, hash: %s", from, to, time, hash);
    }
}
