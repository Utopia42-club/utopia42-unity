using System;
using System.Collections.Generic;
using UnityEngine;

public class VideoBlockObject : MetaBlockObject
{
    private readonly List<GameObject> videos = new List<GameObject>();
    private SnackItem snackItem;
    private Land land;
    private bool canEdit;

    private void Start()
    {
        if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
            CreateIcon();
    }

    public override void OnDataUpdate()
    {
        RenderFaces();
    }

    protected override void DoInitialize()
    {
        RenderFaces();
    }

    public override void Focus()
    {
        UpdateSnacksAndIconObject();
    }

    private void Play()
    {
        foreach (var vid in videos)
            vid.GetComponent<VideoFace>().TogglePlaying();
        UpdateSnacksAndIconObject();
    }

    private void UpdateSnacksAndIconObject()
    {
        bool isPlaying = false;
        foreach (var vid in videos)
        {
            if (vid.GetComponent<VideoFace>().IsPlaying())
            {
                isPlaying = true;
                break;
            }
        }

        if (snackItem != null) snackItem.Remove();
        var lines = new List<string>();
        if (isPlaying)
            lines.Add("Press P to pause");
        else
            lines.Add("Press P to play");

        var icon = GetIconObject();
        if(icon != null)
            icon.SetActive(!isPlaying);

        if (!canEdit)
        {
            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (Input.GetKeyDown(KeyCode.P))
                    Play();
            });
        }
        else
        {
            lines.Add("Press Z for details");
            lines.Add("Press T to toggle preview");
            lines.Add("Press X to delete");

            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetKeyDown(KeyCode.X))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetKeyDown(KeyCode.P))
                    Play();
            });
        }
    }

    public override void UnFocus()
    {
        if (snackItem != null)
        {
            snackItem.Remove();
            snackItem = null;
        }
    }

    private void RenderFaces()
    {
        foreach (var vid in videos)
            DestroyImmediate(vid);
        videos.Clear();

        MediaBlockProperties properties = (MediaBlockProperties)GetBlock().GetProps();
        if (properties != null)
        {
            AddFace(Voxels.Face.BACK, properties.back);
            AddFace(Voxels.Face.FRONT, properties.front);
            AddFace(Voxels.Face.RIGHT, properties.right);
            AddFace(Voxels.Face.LEFT, properties.left);
            AddFace(Voxels.Face.TOP, properties.top);
            AddFace(Voxels.Face.BOTTOM, properties.bottom);
        }
    }

    private void AddFace(Voxels.Face face, MediaBlockProperties.FaceProps props)
    {
        if (props == null) return;
        var go = new GameObject();
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero + ((Vector3)face.direction) * 0.1f;
        var imgFace = go.AddComponent<VideoFace>();
        imgFace.Init(face, props.url, props.width, props.height);
        videos.Add(go);
    }

    private void EditProps()
    {
        var manager = GameManager.INSTANCE;
        var dialog = manager.OpenDialog();
        dialog
            .WithTitle("Video Block Properties")
            .WithContent(MediaBlockEditor.PREFAB);
        var editor = dialog.GetContent().GetComponent<MediaBlockEditor>();
        editor.SetValue(GetBlock().GetProps() as MediaBlockProperties);
        dialog.WithAction("Submit", () =>
        {
            GetBlock().SetProps(editor.GetValue(), land);
            manager.CloseDialog(dialog);
        });
    }
}
