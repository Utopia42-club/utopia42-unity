using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebBridge : MonoBehaviour
{
    private static Dictionary<string, Action<string>> responseListeners = new Dictionary<string, Action<string>>();
    [DllImport("__Internal")]
    private static extern string callOnBridge(string functionName, string parameter);
    [DllImport("__Internal")]
    private static extern string callAsyncOnBridge(string callId, string functionName, string parameter);
    [DllImport("__Internal")]
    private static extern bool isBridgePresent();

    public static bool IsPresent()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer && isBridgePresent();
    }

    public static string PrepareParameters(object parameter)
    {
        var req = new Request<object>();
        req.connection = Settings.ConnectionDetail();
        req.body = parameter;

        return JsonConvert.SerializeObject(req);
    }

    public static T Call<T>(string function, object parameter)
    {
        string requestStr = PrepareParameters(parameter);
        var result = callOnBridge(function, requestStr);
        if (result != null)
            return JsonConvert.DeserializeObject<T>(result);
        return default(T);
    }

    public static void CallAsync<T>(string function, object parameter, Action<T> onDone)
    {
        string paramStr = JsonConvert.SerializeObject(parameter);
        var callId = System.Guid.NewGuid().ToString();
        responseListeners[callId] = (string result) =>
        {
            if (result != null)
                onDone(JsonConvert.DeserializeObject<T>(result));
            else onDone(default(T));
        };
        callAsyncOnBridge(callId, function, paramStr);
    }

    public void Responde(string r)
    {
        var response = JsonConvert.DeserializeObject<Response>(r);
        Action<string> listener;
        if (responseListeners.TryGetValue(response.callId, out listener))
        {
            responseListeners.Remove(response.callId);
            listener(response.body);
        }
        else
        {
            Debug.LogWarning("Invalid callI from java script in: " + r);
        }
    }

}

[Serializable]
class Response
{
    public string callId;
    public string body;
}

[Serializable]
class Request<T>
{
    public T body;
    public ConnectionDetail connection;
}
