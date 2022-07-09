using System;
using System.Collections;
using ReadyPlayerMe;
using Source.Utils;
using UnityEngine;

namespace Source
{
    public class AvatarLoader : TdObjectLoader<string, FailureType>
    {
        private ReadyPlayerMe.AvatarLoader loader = new() {UseAvatarCaching = true};

        protected override IEnumerator GetJob(string url, Action<GameObject> onSuccess, Action<FailureType> onFailure)
        {
            var done = false;

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
            loader = new ReadyPlayerMe.AvatarLoader() {UseAvatarCaching = true};
        }

        protected override bool IsCritical()
        {
            return true;
        }

        public static AvatarLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<AvatarLoader>();
    }
}