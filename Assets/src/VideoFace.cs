using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoFace : MetaFace
{
    private VideoPlayer videoPlayer;

    public void Init(Voxels.Face face, string url, int width, int height)
    {
        MeshRenderer meshRenderer = Initialize(face, width, height);
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.url = url;
        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();
        meshRenderer.material.mainTexture = videoPlayer.texture;
    }

    public void TogglePlaying()
    {
        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    public bool IsPlaying()
    {
        return videoPlayer.isPlaying;
    }
}
