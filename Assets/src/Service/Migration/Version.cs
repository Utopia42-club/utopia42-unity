using System;

internal class Version : IComparable<Version>
{
    private readonly int[] parts;

    public Version(int p1, int p2, int p3)
    {
        parts = new int[] { p1, p2, p3 };
    }

    public Version(string str)
    {
        var strParts = str.Split('.');
        parts = new int[] { int.Parse(strParts[0]), int.Parse(strParts[1]), int.Parse(strParts[2]) };
    }

    public int CompareTo(Version other)
    {
        if (other == null) return 1;

        for (int i = 0; i < 3; i++)
        {
            var diff = parts[0] - other.parts[0];
            if (diff != 0) return diff;
        }
        return 0;
    }

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;

        return CompareTo((Version)obj) == 0;
    }

    public override int GetHashCode()
    {
        int hc = parts.Length;
        foreach (int val in parts)
        {
            hc = unchecked(hc * 314159 + val);
        }
        return hc;
    }

    public override string ToString()
    {
        return string.Join(".", parts);
    }

}
