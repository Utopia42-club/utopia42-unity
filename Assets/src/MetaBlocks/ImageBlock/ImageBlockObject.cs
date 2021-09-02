using System.Collections.Generic;
using UnityEngine;

public class ImageBlockObject : MetaBlockObject
{
    private readonly List<GameObject> images = new List<GameObject>();
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
        if (!canEdit) return;
        if (snackItem != null) snackItem.Remove();
        var lines = new List<string>();
        lines.Add("Press Z for details");
        lines.Add("Press P to toggle preview");
        lines.Add("Press X to delete");
        snackItem = Snack.INSTANCE.ShowLines(lines, () =>
        {
            if (Input.GetKeyDown(KeyCode.Z))
                EditProps();
            if (Input.GetKeyDown(KeyCode.X))
                GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
            if (Input.GetKeyDown(KeyCode.P))
                GetIconObject().SetActive(!GetIconObject().activeSelf);
        });
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
        foreach (var img in images)
            DestroyImmediate(img);
        images.Clear();

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
        var imgFace = go.AddComponent<ImageFace>();
        imgFace.Init(face, props.url, props.width, props.height);
        images.Add(go);
    }

    private void EditProps()
    {
        var manager = GameManager.INSTANCE;
        var dialog = manager.OpenDialog();
        dialog
            .WithTitle("Image Block Properties")
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
