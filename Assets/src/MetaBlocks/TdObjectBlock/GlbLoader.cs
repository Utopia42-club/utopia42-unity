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
                AnimationClip[] clips; // TODO: use glb animation
                var go = Importer.LoadFromBytes(data, ImportSettings, out clips);
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