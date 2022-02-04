using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;

// using Dummiesman;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockObject : MetaBlockObject
    {
        public const ulong DownloadLimitMb = 10;

        private GameObject tdObjectContainer;
        private GameObject tdObject;
        public BoxCollider TdObjectBoxCollider { private set; get; }
        private TdObjectFocusable tdObjectFocusable;

        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private bool ready = false;
        private string currentUrl = "";

        private StateMsg stateMsg = StateMsg.Ok;

        private TdObjectMoveController moveController;

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
            LoadTdObject();
        }

        protected override void DoInitialize()
        {
            LoadTdObject();
        }

        public override void Focus(Voxels.Face face)
        {
            if (!canEdit) return;
            SetupDefaultSnack();
            if (TdObjectBoxCollider != null)
                Player.INSTANCE.ShowTdObjectHighlightBox(TdObjectBoxCollider);
        }

        public void ExitMovingState()
        {
            UpdateProps();
            SetupDefaultSnack();
            if (moveController != null)
            {
                moveController.Detach();
                DestroyImmediate(moveController);
                moveController = null;
            }
        }


        private void SetupDefaultSnack()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            snackItem = Snack.INSTANCE.ShowLines(GetDefaultSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    Player.INSTANCE.HideTdObjectHighlightBox();
                    EditProps();
                }

                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetKeyDown(KeyCode.V) && tdObjectContainer != null)
                {
                    Player.INSTANCE.HideTdObjectHighlightBox();
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
                }

                if (Input.GetButtonDown("Delete"))
                {
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                }
            });
        }

        public void SetToMovingState(bool helpMode = false)
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            if (moveController == null)
            {
                moveController = gameObject.AddComponent<TdObjectMoveController>();
                moveController.Attach(tdObjectContainer.transform, tdObjectContainer.transform,
                    tdObjectContainer.transform);
            }

            var lines = GetMovingSnackLines(helpMode);
            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
                }

                if (Input.GetKeyDown(KeyCode.H))
                    SetToMovingState(!helpMode);
            });
        }

        private static List<string> GetMovingSnackLines(bool helpMode)
        {
            var lines = new List<string>();
            if (helpMode)
            {
                lines.Add("H : exit help");
                lines.Add("W : forward");
                lines.Add("S : backward");
                lines.Add("SPACE : up");
                lines.Add("SHIFT+SPACE : down");
                lines.Add("A : left");
                lines.Add("D : right");
                lines.Add("] : scale up");
                lines.Add("[ : scale down");
                lines.Add("ALT + horizontal mouse movement : rotate around y axis");
                lines.Add("ALT + vertical mouse movement : rotate around player right axis");
            }
            else
            {
                lines.Add("H : help");
            }

            lines.Add("X : exit moving object mode");
            return lines;
        }

        private List<string> GetDefaultSnackLines()
        {
            var lines = new List<string>();
            lines.Add("Press Z for details");
            lines.Add("Press T to toggle preview");
            if (tdObjectContainer != null)
                lines.Add("Press V to move object");
            lines.Add("Press DEL to delete object");
            if (stateMsg != StateMsg.Ok)
                lines.Add("\n" + MetaBlockState.ToString(stateMsg, "3D object"));
            return lines;
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            Player.INSTANCE.HideTdObjectHighlightBox();
        }

        public void UpdateStateAndIcon(StateMsg msg)
        {
            stateMsg = msg;
            if (snackItem != null)
                ((SnackItem.Text) snackItem).UpdateLines(GetDefaultSnackLines());
            if (stateMsg != StateMsg.Ok && stateMsg != StateMsg.Loading)
                CreateIcon(true);
            else
                CreateIcon();
        }

        private void LoadTdObject()
        {
            TdObjectBlockProperties properties = (TdObjectBlockProperties) GetBlock().GetProps();
            if (properties == null) return;

            var scale = properties.scale?.ToVector3() ?? Vector3.one;
            var offset = properties.offset?.ToVector3() ?? Vector3.zero;
            var rotation = properties.rotation?.ToVector3() ?? Vector3.zero;
            var initialPosition = properties.initialPosition?.ToVector3() ?? Vector3.zero;
            var initialScale = properties.initialScale;
            var detectCollision = properties.detectCollision;

            if (currentUrl.Equals(properties.url) && tdObjectContainer != null)
            {
                LoadGameObject(scale, offset, rotation, initialPosition, initialScale, detectCollision);
            }
            else
            {
                DestroyObject();
                UpdateStateAndIcon(StateMsg.Loading);
                StartCoroutine(LoadZip(properties.url, go =>
                {
                    tdObjectContainer = new GameObject("3d object container");
                    tdObjectContainer.transform.SetParent(transform, false);
                    tdObjectContainer.transform.localPosition = Vector3.zero;
                    tdObject = go;
                    tdObject.name = TdObjectBlockType.Name;

                    LoadGameObject(scale, offset, rotation, initialPosition, initialScale, detectCollision);
                }));

                currentUrl = properties.url;
            }
        }

        public void LoadGameObject(Vector3 scale, Vector3 offset, Vector3 rotation, Vector3 initialPosition,
            float initialScale, bool detectCollision)
        {
            if (initialScale == 0) // Not initialized before
            {
                tdObject.transform.localScale = Vector3.one;
                tdObject.transform.localPosition = Vector3.zero;
                var center = GetObjectCenter(tdObject);
                var size = GetObjectSize(center, tdObject);

                var maxD = new[] {size.x, size.y, size.z}.Max();
                if (maxD > 10f)
                {
                    initialScale = 10f / maxD;
                    tdObject.transform.localScale = initialScale * Vector3.one;
                    center = GetObjectCenter(tdObject);
                }
                else
                    initialScale = 1;

                initialPosition = new Vector3(-center.x, -center.y, -center.z);
                InitializeProps(initialPosition, initialScale);
                return;
            }

            if (TdObjectBoxCollider == null)
            {
                TdObjectBoxCollider = tdObject.AddComponent<BoxCollider>();
                tdObjectFocusable = tdObject.AddComponent<TdObjectFocusable>();
                tdObjectFocusable.Initialize(this);
                TdObjectBoxCollider.center = GetObjectCenter(tdObject, false);
                TdObjectBoxCollider.size = GetObjectSize(TdObjectBoxCollider.center, tdObject, false);
            }

            tdObject.transform.SetParent(tdObjectContainer.transform, false);
            tdObject.transform.localScale = initialScale * Vector3.one;
            tdObject.transform.localPosition = initialPosition;

            tdObjectContainer.transform.localScale = scale;
            tdObjectContainer.transform.localPosition = offset;
            tdObjectContainer.transform.eulerAngles = rotation;

            tdObject.layer =
                detectCollision ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("3DColliderOff");
            if (GetBlock().land != null && !InLand(TdObjectBoxCollider))
            {
                DestroyObject();
                UpdateStateAndIcon(StateMsg.OutOfBound);
            }
            else
            {
                UpdateStateAndIcon(StateMsg.Ok);
            }
        }

        private void DestroyObject()
        {
            if (tdObjectFocusable != null)
            {
                tdObjectFocusable.UnFocus();
                tdObjectFocusable = null;
            }

            if (tdObject != null)
            {
                DestroyImmediate(tdObject.gameObject);
                DestroyImmediate(tdObject);
                tdObject = null;
            }

            if (tdObjectContainer != null)
            {
                DestroyImmediate(tdObjectContainer.gameObject);
                DestroyImmediate(tdObjectContainer);
                tdObjectContainer = null;
            }

            TdObjectBoxCollider = null;
        }

        public void InitializeProps(Vector3 initialPosition, float initialScale)
        {
            var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
            props.initialPosition = SerializableVector3.@from(initialPosition);
            props.initialScale = initialScale;
            GetBlock().SetProps(props, land);
        }

        public void UpdateProps()
        {
            var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
            if (tdObjectContainer == null) return;
            props.offset = SerializableVector3.@from(tdObjectContainer.transform.localPosition);
            props.rotation = SerializableVector3.@from(tdObjectContainer.transform.eulerAngles);
            props.scale = SerializableVector3.@from(tdObjectContainer.transform.localScale);
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

                if (props.IsEmpty()) props = null;

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private static Vector3 GetObjectCenter(GameObject loadedObject, bool local = true)
        {
            var center = GetRendererCenter(loadedObject);
            return local ? loadedObject.transform.InverseTransformPoint(center) : center;
        }

        private static Vector3 GetRendererCenter(GameObject loadedObject)
        {
            float
                minX = float.PositiveInfinity,
                maxX = float.NegativeInfinity,
                minY = float.PositiveInfinity,
                maxY = float.NegativeInfinity,
                minZ = float.PositiveInfinity,
                maxZ = float.NegativeInfinity;

            foreach (var child in loadedObject.GetComponentsInChildren<MeshRenderer>())
            {
                var bounds = child.bounds;
                var min = bounds.min;
                var max = bounds.max;

                if (min.x < minX) minX = min.x;
                if (min.y < minY) minY = min.y;
                if (min.z < minZ) minZ = min.z;

                if (max.x > maxX) maxX = max.x;
                if (max.y > maxY) maxY = max.y;
                if (max.z > maxZ) maxZ = max.z;
            }

            return new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
        }

        private static Vector3 GetObjectSize(Vector3 center, GameObject loadedObject, bool local = true)
        {
            var loadedObjectTransform = loadedObject.transform;
            var size = GetRendererSize(local ? loadedObjectTransform.TransformPoint(center) : center, loadedObject);
            return local ? loadedObjectTransform.InverseTransformVector(size) : size;
        }

        private static Vector3 GetRendererSize(Vector3 center, GameObject loadedObject)
        {
            var bounds = new Bounds(center, Vector3.zero);
            foreach (var child in loadedObject.GetComponentsInChildren<MeshRenderer>())
            {
                bounds.Encapsulate(child.bounds);
            }

            return bounds.size;
        }

        private IEnumerator LoadZip(string url, Action<GameObject> consumer)
        {
            using var webRequest = UnityWebRequest.Get(url);
            var op = webRequest.SendWebRequest();

            while (!op.isDone)
            {
                if (webRequest.downloadedBytes > DownloadLimitMb * 1000000)
                    break;
                yield return null;
            }

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.InProgress:
                    DestroyObject();
                    UpdateStateAndIcon(StateMsg.SizeLimit);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"Get for {url} caused Error: {webRequest.error}");
                    DestroyObject();
                    UpdateStateAndIcon(StateMsg.InvalidUrl);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"Get for {url} caused HTTP Error: {webRequest.error}");
                    DestroyObject();
                    UpdateStateAndIcon(StateMsg.InvalidUrl);
                    break;
                case UnityWebRequest.Result.Success:
                    using (var stream = new MemoryStream(webRequest.downloadHandler.data))
                    {
                        try
                        {
                            var go = new OBJLoader().LoadZip(stream);
                            consumer.Invoke(go);
                        }
                        catch (Exception)
                        {
                            DestroyObject();
                            UpdateStateAndIcon(StateMsg.InvalidData);
                        }
                    }

                    break;
            }
        }
    }
}