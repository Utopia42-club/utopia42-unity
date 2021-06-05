using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class IpfsClient
{
    private static readonly string GET_URL = "https://utopia42.club/api/v0/cat?arg=/ipfs/";

    IEnumerator GetFile(string id)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(id))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //JsonUtility.FromJson()
                    break;
            }
        }
    }

}
