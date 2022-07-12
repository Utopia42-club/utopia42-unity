namespace Source.Utils
{
    public static class Constants
    {
        // public static readonly string WebAppBaseURL = "https://dev.utopia42.club";
        public static readonly string WebAppBaseURL = "http://utopia.vitaminhq.ir";
        // public static readonly string WebAppBaseURL = "https://app.utopia42.club";
        // public static readonly string WebAppBaseURL = "http://localhost:4200";

        // public static readonly string ApiURL = "https://demoapi.utopia42.club";
        // public static readonly string ApiURL = "https://utopiapi.vitaminhq.ir";
        // public static readonly string ApiURL = "https://api.utopia42.club";
        public static readonly string ApiURL = "http://192.168.1.196:8080";
        // public static readonly string ApiURL = "http://localhost:8080";

        public static readonly string NetsURL = ApiURL + "/static/networks.json";
        // public static readonly string NetsURL = "https://api.utopia42.club" + "/static/networks.json";
        public static readonly string WebAppRpcURL = WebAppBaseURL + "/rpc";
        
        public static readonly string AvatarDesignerURL = "https://utopia42club.readyplayer.me/avatar";
        public static readonly string IpfsServerURL = "https://utopia42.club/api/v0";
    }
}
