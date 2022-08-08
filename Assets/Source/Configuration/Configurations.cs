using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Source.Configuration
{
    public class Configurations
    {
        private static Configurations cachedInstance;

        public static Configurations Instance
        {
            get
            {
                if (cachedInstance != null)
                    return cachedInstance;
                Dictionary<string, string> conf;
#if UNITY_EDITOR
                var path = Path.Join(Application.dataPath, "config", "env.json");
                conf = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
#else
                conf = WebBridge.Call<Dictionary<string, string>>("getConfigurations", "");
#endif
                return cachedInstance = new Configurations(
                    conf["webAppBaseURL"], conf["apiURL"],
                    conf["ipfsServerURL"], conf["avatarDesignerURL"]
                );
            }
        }

        public readonly string webAppBaseURL;
        public readonly string apiURL;
        public readonly string webAppRpcURL;
        public readonly string ipfsServerURL;
        public readonly string avatarDesignerURL;

        internal Configurations(string webAppBaseURL, string apiURL, string ipfsServerURL, string avatarDesignerURL)
        {
            this.webAppBaseURL = webAppBaseURL;
            this.apiURL = apiURL;
            webAppRpcURL = webAppBaseURL + "/rpc";
            this.ipfsServerURL = ipfsServerURL;
            this.avatarDesignerURL = avatarDesignerURL;
        }

        // public static readonly string WebAppBaseURL = "https://dev.utopia42.club";
        // public static readonly string WebAppBaseURL = "http://utopia.vitaminhq.ir";
        // public static readonly string WebAppBaseURL = "https://app.utopia42.club";
        // public static readonly string WebAppBaseURL = "http://localhost:4200";

        // public static readonly string ApiURL = "https://demoapi.utopia42.club";
        // public static readonly string ApiURL = "https://utopiapi.vitaminhq.ir";
        // public static readonly string ApiURL = "https://api.utopia42.club";
        // public static readonly string ApiURL = "http://localhost:8080";

        // public static readonly string WebAppRpcURL = WebAppBaseURL + "/rpc";
        // public static readonly string IpfsServerURL = "https://utopia42.club/api/v0";

        // public static readonly string AvatarDesignerURL = "https://utopia42club.readyplayer.me/avatar";
    }
}