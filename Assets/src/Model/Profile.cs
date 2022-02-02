using System.Collections.Generic;

namespace src.Model
{
    [System.Serializable]
    public class Profile
    {
        public static readonly Profile LOADING_PROFILE = new Profile();
        public static readonly Profile FAILED_TO_LOAD_PROFILE = new Profile();

        static Profile()
        {
            LOADING_PROFILE.name = "Loading...";
            LOADING_PROFILE.imageUrl = null;
            LOADING_PROFILE.walletId = "";
            LOADING_PROFILE.links = null;
            
            FAILED_TO_LOAD_PROFILE.name = "Failed to load";
            FAILED_TO_LOAD_PROFILE.imageUrl = null;
            FAILED_TO_LOAD_PROFILE.walletId = "";
            FAILED_TO_LOAD_PROFILE.links = null;
        }
        
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
            private static Dictionary<string, Media> medias = new Dictionary<string, Media>(){
                {"TELEGRAM",Media.TELEGRAM},
                {"DISCORD",Media.DISCORD},
                {"FACEBOOK",Media.FACEBOOK},
                {"TWITTER",Media.TWITTER},
                {"INSTAGRAM",Media.INSTAGRAM},
                {"OTHER",Media.OTHER},
            };

            public string link;
            public string media;

            public Media GetMedia()
            {
                return medias[media];
            }

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
}
