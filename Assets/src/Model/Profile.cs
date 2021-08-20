using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class Profile
{
    public string walletId;
    public string name;
    public string bio;
    public List<Link> links;
    public string imageUrl;

    public override int GetHashCode()
    {
        return walletId.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        var other = (Profile)obj;
        return walletId == other.walletId;
    }

    public class Link
    {
        public string link;
        public Media media;

        public class Media
        {
            public static Media TELEGRAM = new Media(0, "Telegram");
            public static Media DISCORD = new Media(1, "Discord");
            public static Media FACEBOOK = new Media(2, "Facebook");
            public static Media TWITTER = new Media(3, "Twitter");
            public static Media INSTAGRAM = new Media(4, "Instagram");
            public static Media OTHER = new Media(5, "Link");

            private int index;
            private string name;

            Media(int index, string name)
            {
                this.index = index;
                this.name = name;
            }

            public int GetIndex()
            {
                return index;
            }

            public string GetName()
            {
                return name;
            }
        }
    }
}
