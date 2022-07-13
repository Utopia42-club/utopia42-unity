using System;
using System.Collections;
using ReadyPlayerMe;
using Source.Utils;
using UnityEngine;

namespace Source
{
    public class AvatarLoader : TdObjectLoader<string, FailureType>
    {
        public static AvatarLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<AvatarLoader>();

        protected override IEnumerator GetJob(string url, Action<GameObject> onSuccess, Action<FailureType> onFailure)
        {
            var done = false;

            var loader = new ReadyPlayerMe.AvatarLoader
            {
                UseAvatarCaching = true
            };

            loader.OnCompleted += (_, args) =>
            {
                done = true;
                onSuccess.Invoke(args.Avatar);
            };
            loader.OnFailed += (_, args) =>
            {
                done = true;
                onFailure.Invoke(args.Type);
            };
            loader.LoadAvatar(url);

            while (!done) yield return null;
        }

        protected override bool IsCritical()
        {
            return true;
        }
    }
}