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
        canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land);
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
        UpdaetSnacks();
    }

    private void UpdaetSnacks()
    {
        if (snackItem != null) snackItem.Remove();
        var lines = new List<string>();
        if (canEdit)
        {
            lines.Add("Press Z for details");
            lines.Add("Press X to delete");
        }

        LinkBlockProperties.FaceProps faceProps = GetFaceProps();
        if (faceProps != null)
        {
            if (faceProps.type == 0)
                lines.Add("Press O to open this web link");
            else if (faceProps.type == 1)
                lines.Add("Press O to open this game link");
        }
        snackItem = Snack.INSTANCE.ShowLines(lines, () =>
        {
            if (canEdit)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetKeyDown(KeyCode.X))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
            }
            if (faceProps != null && Input.GetKeyDown(KeyCode.O))
                OpenLink();
        });
    }

    private void OpenLink()
    {
        LinkBlockProperties.FaceProps faceProps = GetFaceProps();
        if (faceProps.type == 0)
            Application.OpenURL(faceProps.url);
        else if (faceProps.type == 1)
            GameManager.INSTANCE.MovePlayerTo(new Vector3(faceProps.x, faceProps.y, faceProps.z));
    }

    private LinkBlockProperties.FaceProps GetFaceProps()
    {
        LinkBlockProperties properties = (LinkBlockProperties)GetBlock().GetProps();
        if (properties != null)
        {
            LinkBlockProperties.FaceProps faceProps = properties.GetFaceProps();
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

    private void EditProps()
    {
        var manager = GameManager.INSTANCE;
        var dialog = manager.OpenDialog();
        dialog
            .WithTitle("Link Block Properties")
            .WithContent(LinkBlockEditor.PREFAB);
        var editor = dialog.GetContent().GetComponent<LinkBlockEditor>();

        var props = GetBlock().GetProps();
        editor.SetValue(props == null ? null : (props as LinkBlockProperties).GetFaceProps());
        dialog.WithAction("Submit", () =>
        {
            var value = editor.GetValue();
            var props = new LinkBlockProperties(GetBlock().GetProps() as LinkBlockProperties);

            props.SetFaceProps(value);
            if (props.IsEmpty()) props = null;

            GetBlock().SetProps(props, land);
            manager.CloseDialog(dialog);
            UpdaetSnacks();
        });
    }
}
