using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Siccity.GLTFUtility;
using Source.Utils;
using UnityEngine;

namespace Source.MetaBlocks.TdObjectBlock
{
    public class GlbLoader : TdObjectLoader<byte[], int>
    {
        private static readonly ImportSettings ImportSettings = new()
        {
            useLegacyClips = true
        };

        protected override IEnumerator GetJob(GameObject refGo, byte[] data, Action<GameObject> onSuccess,
            Action<int> onFailure)
        {
            if (refGo == null)
            {
                Debug.Log("GlbLoader job skipped since the reference game object has been destroyed");
                yield break;
            }

            var done = false;
            InitTask(data, go =>
            {
                done = true;
                if (refGo == null)
                {
                    Debug.Log(
                        "GlbLoader job success handling skipped since the reference game object has been destroyed");
                    MetaBlockObject.DeepDestroy3DObject(go);
                    return;
                }

                onSuccess.Invoke(go);
            }, () =>
            {
                done = true;
                if (refGo == null)
                {
                    Debug.Log(
                        "GlbLoader job failure handling skipped since the reference game object has been destroyed");
                    return;
                }

                onFailure.Invoke(0);
            });
            while (!done)
                yield return null;
        }

        public static void InitTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            try
            {
                var go = Importer.LoadFromBytes(data, ImportSettings, out var clips);

                if (clips.Length > 0)
                {
                    var anim = go.AddComponent<Animation>();
                    anim.playAutomatically = true;
                    var clip = clips[0];
                    anim.clip = clip;
                    anim.AddClip(clip, clip.name);
                    anim.Play(clip.name);
                }

                onSuccess.Invoke(go);
            }
            catch
            {
                onFailure.Invoke();
            }
        }

        public static GlbLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<GlbLoader>();
    }
}