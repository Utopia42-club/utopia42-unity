using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
// using Dummiesman;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockObject : MetaBlockObject
    {
        private GameObject tdObjectContainer;
        private GameObject tdObject;

        private Vector3? initialPosition;
        private float initialScale = 1;
        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private bool ready = false;
        private string currentUrl = "";

        private void Start()
        {
            if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
                CreateIcon(false);
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
            lines.Add("Press V to move object");

            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetKeyDown(KeyCode.V))
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
            });
        }

        public void SetSnackForMovingObjectMode(bool helpMode = false)
        {
            if (snackItem != null) snackItem.Remove();
            
            var lines = new List<string>();
            if (helpMode)
            {
                lines.Add("W : forward");
                lines.Add("S : backward");
                lines.Add("ALT+W : up");
                lines.Add("ALT+S : down");
                lines.Add("A : left");
                lines.Add("D : right");
                lines.Add("R : rotate");
                lines.Add("] : scale up");
                lines.Add("[ : scale down");
                lines.Add("Del : delete");
                lines.Add("H : exit help");
            }
            else
            {
                lines.Add("H : help");
            }
            lines.Add("X : exit moving object mode");

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
                if (Input.GetKeyDown(KeyCode.X))
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
                if (Input.GetButtonDown("Delete"))
                {
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                }
                if (Input.GetKeyDown(KeyCode.H))
                    SetSnackForMovingObjectMode(!helpMode);
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
            TdObjectBlockProperties properties = (TdObjectBlockProperties) GetBlock().GetProps();
            var scale = properties?.scale?.ToVector3() ?? Vector3.one;
            var offset = properties?.offset?.ToVector3() ?? Vector3.zero;
            var rotation = properties?.rotation?.ToVector3() ?? Vector3.zero;

            if (properties != null)
            {
                if (currentUrl.Equals(properties.url) && tdObjectContainer != null)
                {
                    LoadGameObject(scale, offset, rotation);
                }
                else
                {
                    DestroyImmediate(tdObjectContainer);
                    tdObjectContainer = null;
                    tdObject = null;
                    initialPosition = null;

                    StartCoroutine(LoadZip(properties.url, go =>
                    {
                        tdObjectContainer = new GameObject();
                        tdObjectContainer.transform.SetParent(transform, false);
                        tdObjectContainer.transform.localPosition = Vector3.zero;
                        tdObject = go;
                        
                        LoadGameObject(scale, offset, rotation);
                    }));

                    currentUrl = properties.url;
                }
            }
        }

        public void LoadGameObject(Vector3 scale, Vector3 offset, Vector3 rotation)
        {
            tdObject.transform.localScale = scale * initialScale;
            if (initialPosition == null)
            {
                var center = GetObjectCenter(tdObject);
                var size = GetObjectSize(tdObject, center);
                var minY = center.y - size.y / 2;

                var maxD = new[] {size.x, size.y, size.z}.Max();
                if (maxD > 3)
                {
                    initialScale = 3f / maxD;
                    tdObject.transform.localScale *= initialScale;
                    center = GetObjectCenter(tdObject);
                    size = GetObjectSize(tdObject, center); 
                    minY = center.y - size.y / 2;
                }

                Debug.Log("Object loaded, size = " + size);
                tdObject.transform.SetParent(tdObjectContainer.transform, false);
                initialPosition = new Vector3(-center.x, -minY + 1, -center.z);
            }

            tdObject.transform.localPosition = (Vector3) initialPosition + offset;
            tdObjectContainer.transform.eulerAngles = rotation;

            var bc = getBoxCollider(tdObject);
            
            var land = GetBlock().land;
            if (land != null && !IsInLand(bc))
            {
                Debug.Log("Overflow! land id: " + land.id);
                CreateIcon(true);
                DestroyImmediate(tdObjectContainer);
                tdObject = null;
                tdObjectContainer = null;
            }
        }

        private BoxCollider getBoxCollider(GameObject tdObject)
        {
            var bc = tdObject.GetComponent<BoxCollider>();
            if (bc == null) bc = tdObject.AddComponent<BoxCollider>();
            bc.center = GetObjectCenter(tdObject, false);
            bc.size = GetObjectSize(tdObject, bc.center, false);
            return bc;
        }


        public void UpdateProps()
        {
            var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
            if(tdObjectContainer == null) return;
            props.offset = SerializableVector3.@from(tdObject.transform.localPosition - (Vector3) initialPosition);
            props.rotation = SerializableVector3.@from(tdObjectContainer.transform.eulerAngles);
            props.scale = SerializableVector3.@from(tdObject.transform.localScale / initialScale);
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
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
                props.UpdateProps(value);

                if (props.IsEmpty()) props = null; // TODO: now?

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private static Vector3 GetObjectCenter(GameObject loadedObject, bool usingMeshRenderer = true)
        {
            var center = Vector3.zero;

            foreach (Transform child in loadedObject.transform)
            {
                if (usingMeshRenderer)
                {
                    var r = child.gameObject.GetComponent<MeshRenderer>();
                    if (r != null)
                        center += r.bounds.center;    
                }
                else
                {
                    var r = child.gameObject.GetComponent<MeshFilter>().mesh;
                    if (r != null)
                        center += r.bounds.center;   
                }
            }

            return center / loadedObject.transform.childCount;
        }

        private static Vector3 GetObjectSize(GameObject loadedObject, Vector3 center, bool usingMeshRenderer = true)
        {
            var bounds = new Bounds(center, Vector3.zero);
            foreach (Transform child in loadedObject.transform)
            {
                if (usingMeshRenderer)
                {
                    var r = child.gameObject.GetComponent<MeshRenderer>();
                    if (r != null)
                        bounds.Encapsulate(r.bounds);   
                }
                else
                {
                    var r = child.gameObject.GetComponent<MeshFilter>().mesh;
                    if (r != null)
                        bounds.Encapsulate(r.bounds);
                }
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
            if (tdObjectContainer == null) return;
            tdObjectContainer.transform.Rotate(3 * Vector3.up);
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