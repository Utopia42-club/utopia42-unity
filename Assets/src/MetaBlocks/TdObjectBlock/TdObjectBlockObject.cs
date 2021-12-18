using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dummiesman;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockObject : MetaBlockObject
    {
        private GameObject tdObject;
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
            loadTdObject();
        }

        protected override void DoInitialize()
        {
            loadTdObject();
        }

        public override void Focus(Voxels.Face face)
        {
            if (!canEdit) return;
            if (snackItem != null) snackItem.Remove();
            var lines = new List<string>();
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

        private void loadTdObject()
        {
            DestroyImmediate(tdObject);

            TdObjectBlockProperties properties = (TdObjectBlockProperties) GetBlock().GetProps();

            if (properties != null)
            {
                StartCoroutine(LoadZip(properties.url,
                    go =>
                    {
                        var center = GetObjectCenter(go);
                        var size = GetObjectSize(go, center);
                        Debug.Log("Object loaded, size = " + size);
                        go.transform.SetParent(transform, false);
                        go.transform.localPosition = -center + 2*Vector3.up; // TODO: fix height
                        tdObject = go;
                    }));
            }
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("3D Object Properties")
                .WithContent(TdObjectBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<TdObjectBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props == null ? null : (props as TdObjectBlockProperties).url);
            dialog.WithAction("Submit", () =>
            {
                var url = editor.GetValue();
                var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
                props.SetProps(url);

                if (props.IsEmpty()) props = null; // TODO: now?

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private static Vector3 GetObjectCenter(GameObject loadedObject)
        {
            var center = Vector3.zero;

            foreach (Transform child in loadedObject.transform)
            {
                var r = child.gameObject.GetComponent<MeshRenderer>();
                if (r != null)
                    center += r.bounds.center;
            }
            return center / loadedObject.transform.childCount;
        }
    
        private static Vector3 GetObjectSize(GameObject loadedObject, Vector3 center)
        {
            var bounds = new Bounds(center, Vector3.zero);
            foreach (Transform child in loadedObject.transform)
            {
                var r = child.gameObject.GetComponent<MeshRenderer>();
                if (r != null)
                    bounds.Encapsulate(r.bounds);
            }
            return bounds.size;
        }

        private static IEnumerator LoadZip(string url, Action<GameObject> consumer)
        {
            using var webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Get for {url} caused Error: {webRequest.error}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"Get for {url} caused HTTP Error: {webRequest.error}");
                    break;
                case UnityWebRequest.Result.Success:
                    using (var stream = new MemoryStream(webRequest.downloadHandler.data))
                    {
                        consumer.Invoke(new OBJLoader().LoadZip(stream));
                    }

                    break;
            }
        }
    }
}