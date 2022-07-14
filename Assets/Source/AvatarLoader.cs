using System;
using System.Collections;
using ReadyPlayerMe;
using Source.MetaBlocks;
using Source.Utils;
using UnityEngine;

namespace Source
{
    public class AvatarLoader : TdObjectLoader<string, FailureType>
    {
        protected override IEnumerator GetJob(GameObject refGo, string url, Action<GameObject> onSuccess,
            Action<FailureType> onFailure)
        {
            if (refGo == null)
            {
                Debug.Log("AvatarLoader job skipped since the reference game object has been destroyed");
                yield break;
            }

            var done = false;

            var loader = new ReadyPlayerMe.AvatarLoader
            {
                UseAvatarCaching = true
            };

            loader.OnCompleted += (_, args) =>
            {
                done = true;
                if (refGo == null)
                {
                    Debug.Log("AvatarLoader job success handling skipped since the reference game object has been destroyed");
                    MetaBlockObject.DeepDestroy3DObject(args.Avatar);
                    return;
                }

                onSuccess.Invoke(args.Avatar);
            };
            loader.OnFailed += (_, args) =>
            {
                done = true;
                if (refGo == null)
                {
                    Debug.Log("AvatarLoader job failure handling skipped since the reference game object has been destroyed");
                    return;
                }

                onFailure.Invoke(args.Type);
            };
            loader.LoadAvatar(url);

            while (!done) yield return null;
        }

        protected override bool IsCritical()
        {
            return true;
        }

        public static AvatarLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<AvatarLoader>();
    }
}