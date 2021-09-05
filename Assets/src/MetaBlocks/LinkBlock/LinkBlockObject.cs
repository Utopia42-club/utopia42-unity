using System.Collections.Generic;
using UnityEngine;

public class LinkBlockObject : MetaBlockObject
{
    private SnackItem snackItem;
    private Land land;
    private bool canEdit;
    private bool ready = false;

    private void Start()
    {
        if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
            CreateIcon();
        ready = true;
    }

    public override bool IsReady()
    {
        return ready;
    }

    public override void OnDataUpdate()
    {
    }

    protected override void DoInitialize()
    {
    }


    public override void Focus(Voxels.Face face)
    {
        if (!canEdit) return;
        UpdaetSnacks(face);
    }

    private void UpdaetSnacks(Voxels.Face face)
    {
        if (snackItem != null) snackItem.Remove();
        var lines = new List<string>();
        lines.Add("Press Z for details");

        LinkBlockProperties.FaceProps faceProps = GetFaceProps(face);
        if (faceProps != null)
        {
            if (faceProps.type == 0)
                lines.Add("Press O to open this web link");
            else if (faceProps.type == 1)
                lines.Add("Press O to open this game link");
        }

        lines.Add("Press X to delete");
        snackItem = Snack.INSTANCE.ShowLines(lines, () =>
        {
            if (Input.GetKeyDown(KeyCode.Z))
                EditProps(face);
            if (Input.GetKeyDown(KeyCode.X))
                GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
            if (faceProps != null && Input.GetKeyDown(KeyCode.O))
                OpenLink(face);
        });
    }

    private void OpenLink(Voxels.Face face)
    {
        LinkBlockProperties.FaceProps faceProps = GetFaceProps(face);
        if (faceProps.type == 0)
            Application.OpenURL(faceProps.url);
        else if (faceProps.type == 1)
            GameManager.INSTANCE.MovePlayerTo(new Vector3(faceProps.x, faceProps.y, faceProps.z));
    }

    private LinkBlockProperties.FaceProps GetFaceProps(Voxels.Face face)
    {
        LinkBlockProperties properties = (LinkBlockProperties)GetBlock().GetProps();
        if (properties != null)
        {
            LinkBlockProperties.FaceProps faceProps = properties.GetFaceProps(face);
            return faceProps;
        }
        return null;
    }

    public override void UnFocus()
    {
        if (snackItem != null)
        {
            snackItem.Remove();
            snackItem = null;
        }
    }

    private void EditProps(Voxels.Face face)
    {
        var manager = GameManager.INSTANCE;
        var dialog = manager.OpenDialog();
        dialog
            .WithTitle("Link Block Properties")
            .WithContent(LinkBlockEditor.PREFAB);
        var editor = dialog.GetContent().GetComponent<LinkBlockEditor>();

        var props = GetBlock().GetProps();
        editor.SetValue(props == null ? null : (props as LinkBlockProperties).GetFaceProps(face));
        dialog.WithAction("Submit", () =>
        {
            var value = editor.GetValue();
            var props = new LinkBlockProperties(GetBlock().GetProps() as LinkBlockProperties);

            props.SetFaceProps(face, value);
            if (props.IsEmpty()) props = null;

            GetBlock().SetProps(props, land);
            manager.CloseDialog(dialog);
            UpdaetSnacks(face);
        });
    }
}
