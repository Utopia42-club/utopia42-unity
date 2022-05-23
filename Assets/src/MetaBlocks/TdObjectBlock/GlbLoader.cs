using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Siccity.GLTFUtility;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class GlbLoader : MonoBehaviour
    {
        private static readonly ImportSettings ImportSettings = new ImportSettings
        {
            animationSettings =
            {
                useLegacyClips = true
            }
        };


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