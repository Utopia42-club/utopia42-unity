using System.Collections;
using src.MetaBlocks;
using src.MetaBlocks.VideoBlock;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace src
{
    public class VideoFace : MetaFace
    {
        private VideoPlayer videoPlayer;
        private float prevTime;
        private bool previewing = true;
        private bool prepared = false;
        private VideoBlockObject block;

        public void Init(MeshRenderer meshRenderer, string url, float prevTime, VideoBlockObject block)
        {
            this.block = block;
            previewing = true;
            prepared = false;
            block.UpdateState(State.Loading);
            this.prevTime = prevTime;
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.url = url;
            videoPlayer.playOnAwake = false;
            videoPlayer.Pause();
            videoPlayer.Prepare();
            // videoPlayer.errorReceived += OnError; // Editor crashes here
            videoPlayer.prepareCompleted += PrepareCompeleted;
            meshRenderer.sharedMaterial.mainTexture = videoPlayer.texture;
        }

        public void PlaceHolderInit(MeshRenderer renderer, bool error)
        {
            renderer.sharedMaterial.mainTexture = Blocks.VideoBlockType.GetIcon(error).texture;
        }

        private void Mute(bool m)
        {
            for (int i = 0; i < videoPlayer.length; i++)
                videoPlayer.SetDirectAudioMute((ushort) i, m);
        }

        private void PrepareCompeleted(VideoPlayer vp)
        {
            StartCoroutine(Seek());
            videoPlayer.prepareCompleted -= PrepareCompeleted;
        }
        
        private void OnError(VideoPlayer vp, string msg)
        {
            block.UpdateState(State.InvalidUrlOrData);
        }

        private IEnumerator Seek()
        {
            Mute(true);
            yield return null;
            videoPlayer.time = prevTime;
            videoPlayer.Play();
            yield return null;

            while (videoPlayer.time < prevTime + 0.01 && videoPlayer.time > prevTime - 0.01)
                yield return null;
            videoPlayer.Pause();
            yield return null;
            Mute(false);
            prepared = true;
            block.UpdateState(State.Ok);
        }


        private IEnumerator DoOnNext(UnityAction a)
        {
            yield return null;
            a.Invoke();
        }

        public void TogglePlaying()
        {
            if (!prepared) return;

            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
            {
                if (previewing)
                {
                    videoPlayer.time = 0;
                    previewing = false;
                }

                videoPlayer.Play();
            }
        }

        public bool IsPrepared()
        {
            return prepared && videoPlayer.isPrepared;
        }

        public bool IsPlaying()
        {
            return prepared && videoPlayer.isPlaying;
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                Destroy(videoPlayer.texture);
                videoPlayer = null;
            }

            base.OnDestroy();
        }
    }
}