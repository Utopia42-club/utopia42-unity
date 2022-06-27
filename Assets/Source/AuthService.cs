using System;

namespace Source
{
    public class AuthService
    {
        public static void GetAuthToken(Action<string> onDone, bool forceValid = false)
        {
            if (!WebBridge.IsPresent())
                return; //FIXME
            WebBridge.CallAsync<string>("getAuthToken", forceValid, onDone.Invoke);
        }
    }
}