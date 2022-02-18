using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using src.Canvas;
using src.Model;
using UnityEngine;

namespace src
{
    public class WebBridge : MonoBehaviour
    {
        private static Dictionary<string, Action<string>> responseListeners = new Dictionary<string, Action<string>>();

        [DllImport("__Internal")]
        private static extern string callOnBridge(string functionName, string parameter);

        [DllImport("__Internal")]
        private static extern string callAsyncOnBridge(string id, string functionName, string parameter);

        [DllImport("__Internal")]
        private static extern bool isBridgePresent();

        public static bool IsPresent()
        {
            return Application.platform == RuntimePlatform.WebGLPlayer && isBridgePresent();
        }

        public static string PrepareParameters(object parameter)
        {
            var req = new UnityToWebRequest<object>();
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
            var id = System.Guid.NewGuid().ToString();
            responseListeners[id] = (string result) =>
            {
                if (result != null)
                    onDone(JsonConvert.DeserializeObject<T>(result));
                else onDone(default(T));
            };
            callAsyncOnBridge(id, function, paramStr);
        }

        public void Respond(string r)
        {
            var response = JsonConvert.DeserializeObject<Response>(r);
            Action<string> listener;
            if (responseListeners.TryGetValue(response.id, out listener))
            {
                responseListeners.Remove(response.id);
                listener(response.body);
            }
            else
            {
                Debug.LogWarning("Invalid callI from java script in: " + r);
            }
        }

        public void Request(string req)
        {
            var request = JsonConvert.DeserializeObject<WebToUnityRequest>(req);
            var components = GameObject.Find(request.objectName).GetComponents<Component>();
            var component = components.FirstOrDefault(t => t.GetType().Name == request.objectName);
            if (component == null)
            {
                Debug.LogError("No component found with name: " + request.objectName);
                return;
            }

            var method = component.GetType().GetMethod(request.methodName);
            Response response;
            try
            {
                var result = request.parameter != null
                    ? (string) method!.Invoke(component, new object[] {request.parameter})
                    : (string) method!.Invoke(component, new object[] { });
                response = new Response
                {
                    id = request.id,
                    body = result
                };
            }
            catch (Exception e)
            {
                response = new Response
                {
                    id = request.id,
                    error = e.Message
                };
            }
            Call<string>("respond", JsonConvert.SerializeObject(response));
        }
    }

    [Serializable]
    class UnityToWebRequest<T>
    {
        public T body;
        public ConnectionDetail connection;
    };

    [Serializable]
    class Response
    {
        public string id;
        public string body;
        public string error;
    }

    [Serializable]
    class WebToUnityRequest
    {
        public string id;
        public string objectName;
        public string methodName;
        public string parameter;
    }
}