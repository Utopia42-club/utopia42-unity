using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dummiesman;
// using Dummiesman;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Vector3 = UnityEngine.Vector3;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockObject : MetaBlockObject
    {
        private GameObject tdObject;
        private Vector3 initialPosition;
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
            SetSnackForPlayingMode();
        }

        public void SetSnackForPlayingMode()
        {
            if (snackItem != null) snackItem.Remove();
            
            var lines = new List<string>();
            lines.Add("Press Z for details");
            lines.Add("Press T to toggle preview");
            lines.Add("Press X to delete");
            lines.Add("Press V to move object");

            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetKeyDown(KeyCode.X))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetKeyDown(KeyCode.V))
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
            });
        }
        
        public void SetSnackForMovingObjectMode()
        {
            if (snackItem != null) snackItem.Remove();
            
            var lines = new List<string>();
            lines.Add("Press W to move forward");
            lines.Add("Press S to move backward");
            lines.Add("Press ALT+W to move up");
            lines.Add("Press ALT+S to move down");
            lines.Add("Press A to move left");
            lines.Add("Press D to move right");
            lines.Add("Press R to rotate");
            lines.Add("Press ] to scale up");
            lines.Add("Press [ to scale down");
            lines.Add("Press V to exit moving object mode");


            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                
                if (Input.GetKey(KeyCode.R)) RotateAroundY();
                if (Input.GetKey(KeyCode.A)) MoveLeft();
                if (Input.GetKey(KeyCode.D)) MoveRight();
                if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftAlt)) MoveUp();
                if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftAlt)) MoveDown();
                if (Input.GetKey(KeyCode.W)) MoveForward();
                if (Input.GetKey(KeyCode.S)) MoveBackward();
                if (Input.GetKey(KeyCode.RightBracket)) ScaleUp();
                if (Input.GetKey(KeyCode.LeftBracket)) ScaleDown();
                if (Input.GetKeyDown(KeyCode.V))
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
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
            tdObject = null;
            transform.eulerAngles = Vector3.zero;

            TdObjectBlockProperties properties = (TdObjectBlockProperties) GetBlock().GetProps();
            var scale = properties != null ? properties.scale : Vector3.one;
            var offset = properties != null ? properties.offset : Vector3.zero;
            var rotation = properties != null ? properties.rotation : Vector3.zero;

            if (properties != null)
            {
                StartCoroutine(LoadZip(properties.url,
                    go =>
                    {
                        var center = GetObjectCenter(go);
                        var size = GetObjectSize(go, center);
                        var bc = go.AddComponent<BoxCollider>(); // TODO ?
                        bc.center = center;
                        bc.size = size;

                        var minY = center.y - size.y / 2;
                        Debug.Log("Object loaded, size = " + size);
                        go.transform.SetParent(transform, false);
                        initialPosition = new Vector3(-center.x, -minY + 1, -center.z);
                        go.transform.localPosition = initialPosition + offset;
                        go.transform.localScale = scale;
                        transform.eulerAngles = rotation;
                        tdObject = go;
                    }));
            }
        }

        public void UpdateProps()
        {
            var props = GetBlock().GetProps() as TdObjectBlockProperties;
            props.offset = tdObject.transform.localPosition - initialPosition;
            props.rotation = transform.eulerAngles;
            props.scale = tdObject.transform.localScale;
            GetBlock().SetProps(props, land);
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
            editor.SetValue(props == null ? null : props as TdObjectBlockProperties);
            dialog.WithAction("Submit", () =>
            {
                var value = editor.GetValue();
                var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
                props.UpdateProps(value);

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

        public void RotateAroundY()
        {
            if (tdObject == null) return;
            transform.Rotate(3 * Vector3.up);
        }

        public void MoveLeft()
        {
            if (tdObject == null) return;
            tdObject.transform.position += Player.INSTANCE.transform.right * -0.1f;
        }

        public void MoveRight()
        {
            if (tdObject == null) return;
            tdObject.transform.position += Player.INSTANCE.transform.right * 0.1f;
        }

        public void MoveForward()
        {
            if (tdObject == null) return;
            tdObject.transform.position += Player.INSTANCE.transform.forward * 0.1f;
        }

        public void MoveBackward()
        {
            if (tdObject == null) return;
            tdObject.transform.position += Player.INSTANCE.transform.forward * -0.1f;
        }

        public void MoveUp()
        {
            if (tdObject == null) return;
            tdObject.transform.position += Vector3.up * 0.1f;
        }

        public void MoveDown()
        {
            if (tdObject == null) return;
            tdObject.transform.position += Vector3.down * 0.1f;
        }

        public void ScaleUp()
        {
            if (tdObject == null) return;
            tdObject.transform.localScale *= 1.1f;
        }

        public void ScaleDown()
        {
            if (tdObject == null) return;
            tdObject.transform.localScale *= 0.9f;
        }
    }
}