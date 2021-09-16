using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class VideoFace : MetaFace
{
    private VideoPlayer videoPlayer;
    public readonly UnityEvent<bool> loading = new UnityEvent<bool>();
    private float prevTime;
    private bool previewing = true;
    private bool prepared = false;

    private void Start()
    {
        //Init(Voxels.Face.FRONT, "https://www.rmp-streaming.com/media/big-buck-bunny-360p.mp4", 2, 2, 10);
    }

    public void Init(Voxels.Face face, string url, int width, int height, float prevTime)
    {
        previewing = true;
        prepared = false;
        loading.Invoke(true);
        MeshRenderer meshRenderer = Initialize(face, width, height);
        this.prevTime = prevTime;
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.url = url;
        videoPlayer.playOnAwake = false;
        videoPlayer.Pause();
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += PrepareCompeleted;
        meshRenderer.material.mainTexture = videoPlayer.texture;
    }

    private void Mute(bool m)
    {
        for (int i = 0; i < videoPlayer.length; i++)
            videoPlayer.SetDirectAudioMute((ushort)i, m);
    }

    private void PrepareCompeleted(VideoPlayer vp)
    {


        //videoPlayer.Pause();

        //if (previewing)
        // {
        StartCoroutine(Seek());
        // }
        videoPlayer.prepareCompleted -= PrepareCompeleted;
    }

    private IEnumerator Seek()
    {
        Mute(true);
        yield return null;
        //prevTime = Mathf.Min(Mathf.Max(0, prevTime), (float)videoPlayer.length);
        videoPlayer.time = prevTime;
        videoPlayer.Play();
        yield return null;

        while (videoPlayer.time < prevTime + 0.01 && videoPlayer.time > prevTime - 0.01)
            yield return null;
        videoPlayer.Pause();
        yield return null;
        Mute(false);
        prepared = true;
        loading.Invoke(false);

        //videoPlayer.seekCompleted += SeekCompeleted;
        //        videoPlayer.frameReady += FrameReady;
        //    videoPlayer.frame = (long)(prevTime * videoPlayer.frameRate);//Mathf.Min(Mathf.Max(0, ), (float)videoPlayer.length);
        //      videoPlayer.Prepare();
    }


    private IEnumerator DoOnNext(UnityAction a)
    {
        yield return null;
        a.Invoke();
    }

    private void FrameReady(VideoPlayer vp, long frameIdx)
    {
        Debug.Log("Frame Ready!!");
    }

    private void SeekCompeleted(VideoPlayer vp)
    {
        Debug.Log("Seek Compeleted!!");
        if (previewing)
        {
            videoPlayer.seekCompleted -= SeekCompeleted;

            videoPlayer.Play();
            StartCoroutine(DoOnNext(() =>
            {
                videoPlayer.Pause();

                prepared = true;
                loading.Invoke(false);
            }));
        }
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
}
