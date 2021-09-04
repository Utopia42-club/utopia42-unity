using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class VideoFace : MetaFace
{
    private VideoPlayer videoPlayer;
    public readonly UnityEvent<bool> loading = new UnityEvent<bool>();


    public void Init(Voxels.Face face, string url, int width, int height)
    {
        loading.Invoke(true);
        MeshRenderer meshRenderer = Initialize(face, width, height);
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.url = url;
        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();
        videoPlayer.Play();
        videoPlayer.prepareCompleted += PrepareCompeleted;
        meshRenderer.material.mainTexture = videoPlayer.texture;
    }

    private void PrepareCompeleted(VideoPlayer vp)
    {
        videoPlayer.Pause();
        loading.Invoke(false);
    }

    public void TogglePlaying()
    {
        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    public bool IsPrepared()
    {
        return videoPlayer.isPrepared;
    }

    public bool IsPlaying()
    {
        return videoPlayer.isPlaying;
    }
}
