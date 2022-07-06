using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Source.Canvas;
using Source.Model;
using Source.Ui.Login;
using Source.Ui.Menu;
using UnityEngine;
using UnityEngine.Events;
using Object = System.Object;

namespace Source
{
    public class WebBridge : MonoBehaviour
    {
        private static Dictionary<string, Action<string>> responseListeners = new Dictionary<string, Action<string>>();

        private static Dictionary<string, Action> unityToWebResponseTeardownLogics = new Dictionary<string, Action>();

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
            req.connection = AuthService.ConnectionDetail();
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
            string paramStr = PrepareParameters(parameter);
            var id = Guid.NewGuid().ToString();
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

        public void UpdateClipboard(string content)
        {
            GUIUtility.systemCopyBuffer = content;
        }

        public string ReadClipboard()
        {
            return GUIUtility.systemCopyBuffer;
        }
        
        public void Request(string req)
        {
            var request = JsonConvert.DeserializeObject<WebToUnityRequest>(req);

            if (request.command == "cancel")
            {
                if (unityToWebResponseTeardownLogics.ContainsKey(request.id))
                    unityToWebResponseTeardownLogics[request.id].Invoke();
            }
            else
            {
                var components = GameObject.Find(request.objectName).GetComponents<Component>();
                var component = components.FirstOrDefault(t => t.GetType().Name == request.objectName);
                if (component == null)
                {
                    Debug.LogError("No component found with name: " + request.objectName);
                    return;
                }

                var method = component.GetType().GetMethod(request.methodName);
                if (method == null)
                {
                    Debug.LogError("No method found with name: " + request.methodName);
                    return;
                }

                try
                {
                    var result = request.parameter != null
                        ? method!.Invoke(component, new object[] {request.parameter})
                        : method!.Invoke(component, new object[] { });

                    if (result is UnityEvent<object> unityEvent)
                    {
                        UnityAction<object> listener = res =>
                        {
                            var response = new Response {id = request.id, body = JsonConvert.SerializeObject(res)};
                            SendResponse(response);
                        };
                        unityEvent.AddListener(listener);
                        unityToWebResponseTeardownLogics[request.id] = () => unityEvent.RemoveListener(listener);
                    }
                    else
                    {
                        var response = new Response
                        {
                            id = request.id,
                            body = JsonConvert.SerializeObject(result)
                        };
                        Call<string>("respond", JsonConvert.SerializeObject(response));

                        var completeResponse = new Response
                        {
                            id = request.id,
                            command = "complete"
                        };
                        Call<string>("respond", JsonConvert.SerializeObject(completeResponse));
                    }
                }
                catch (Exception e)
                {
                    var response = new Response
                    {
                        id = request.id,
                        error = e.GetBaseException().Message
                    };
                    Call<string>("respond", JsonConvert.SerializeObject(response));
                }
            }
        }

        private static void SendResponse(Response response)
        {
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
        public string command;
    }

    [Serializable]
    class WebToUnityRequest
    {
        public string id;
        public string objectName;
        public string methodName;
        public string parameter;
        public string command;
    }
}