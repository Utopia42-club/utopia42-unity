using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockObject : MetaBlockObject
    {
        public const ulong DownloadLimitMb = 10;

        private GameObject tdObjectContainer;
        private GameObject tdObject;
        public Collider TdObjectCollider { private set; get; }
        public MeshRenderer ColliderRenderer { private set; get; }
        public MeshRenderer ColliderRendererFoSelection { private set; get; }
        private TdObjectFocusable tdObjectFocusable;

        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private bool ready = false;
        private string currentUrl = "";

        private StateMsg stateMsg = StateMsg.Ok;

        private TdObjectMoveController moveController;
        private Player player;

        private void Start()
        {
            if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
                CreateIcon();
            ready = true;
            player = Player.INSTANCE;
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
            if (TdObjectCollider != null)
                ShowFocusHighlight();
        }
        
        public override void ShowFocusHighlight()
        {
            if (TdObjectCollider is BoxCollider boxCollider)
            {
                var colliderTransform = boxCollider.transform;
                player.tdObjectHighlightBox.transform.rotation = colliderTransform.rotation;

                var size = boxCollider.size;
                var minPos = boxCollider.center - size / 2;

                var gameObjectTransform = boxCollider.gameObject.transform;
                size.Scale(gameObjectTransform.localScale);
                size.Scale(gameObjectTransform.parent.localScale);

                player.tdObjectHighlightBox.localScale = size;
                player.tdObjectHighlightBox.position = colliderTransform.TransformPoint(minPos);
                player.tdObjectHighlightBox.gameObject.SetActive(true);
            }
            else if (ColliderRenderer != null)
            {
                ColliderRenderer.enabled = true;
            }
        }

        public override void RemoveFocusHighlight()
        {
            if (ColliderRenderer != null)
            {
                ColliderRenderer.enabled = false;
                return;
            }
            player.tdObjectHighlightBox.gameObject.SetActive(false);
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

            snackItem = Snack.INSTANCE.ShowLines(GetFaceSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    RemoveFocusHighlight();
                    EditProps();
                }

                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetKeyDown(KeyCode.V) && tdObjectContainer != null)
                {
                    RemoveFocusHighlight();
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
                lines.Add("R + horizontal mouse movement : rotate around y axis");
                lines.Add("R + vertical mouse movement : rotate around player right axis");
            }
            else
            {
                lines.Add("H : help");
            }

            lines.Add("X : exit moving object mode");
            return lines;
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            RemoveFocusHighlight();
        }

        public override void UpdateStateAndIcon(StateMsg msg, Voxels.Face face = null)
        {
            stateMsg = msg;
            if (snackItem != null)
                ((SnackItem.Text) snackItem).UpdateLines(GetFaceSnackLines());
            if (stateMsg != StateMsg.Ok && stateMsg != StateMsg.Loading)
                CreateIcon(true);
            else
                CreateIcon();
        }

        protected override List<string> GetFaceSnackLines(Voxels.Face face = null)
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

        private void LoadTdObject()
        {
            TdObjectBlockProperties properties = (TdObjectBlockProperties) GetBlock().GetProps();
            if (properties == null) return;

            var scale = properties.scale?.ToVector3() ?? Vector3.one;
            var offset = properties.offset?.ToVector3() ?? Vector3.zero;
            var rotation = properties.rotation?.ToVector3() ?? Vector3.zero;
            var initialPosition = properties.initialPosition?.ToVector3() ?? Vector3.zero;

            if (currentUrl.Equals(properties.url) && tdObjectContainer != null)
            {
                LoadGameObject(scale, offset, rotation, initialPosition, properties.initialScale,
                    properties.detectCollision);
            }
            else
            {
                DestroyObject();
                UpdateStateAndIcon(StateMsg.Loading);
                var reinitialize = !currentUrl.Equals("") || properties.initialScale == 0;
                currentUrl = properties.url;
                StartCoroutine(LoadBytes(properties.url, properties.type, go =>
                {
                    TdObjectCollider = null;
                    ColliderRenderer = null;
                    ColliderRendererFoSelection = null;
                    
                    tdObjectContainer = new GameObject("3d object container");
                    tdObjectContainer.transform.SetParent(transform, false);
                    tdObjectContainer.transform.localPosition = Vector3.zero;
                    tdObjectContainer.transform.localScale = Vector3.one;
                    tdObjectContainer.transform.eulerAngles = Vector3.zero;
                    tdObject = go;
                    tdObject.name = TdObjectBlockType.Name;

                    LoadGameObject(scale, offset, rotation, initialPosition, properties.initialScale,
                        properties.detectCollision, reinitialize);

                    // replace box collider with mesh collider if any colliders are defined in the glb object
                    var colliderTransform = tdObject.GetComponentsInChildren<Transform>()
                        .FirstOrDefault(t => t.name.EndsWith("_collider"));
                    if (colliderTransform == null ||
                        properties.type != TdObjectBlockProperties.TdObjectType.GLB) return;
                    Destroy(tdObjectFocusable);
                    Destroy(TdObjectCollider);

                    colliderTransform.localScale = 1.01f * colliderTransform.localScale;

                    var clone = Instantiate(colliderTransform.gameObject, colliderTransform.transform.parent);
                    ColliderRendererFoSelection = clone.GetComponent<MeshRenderer>();
                    ColliderRendererFoSelection.enabled = false;
                    ColliderRendererFoSelection.material = Player.INSTANCE.HighlightMaterial;

                    ColliderRenderer = colliderTransform.gameObject.GetComponent<MeshRenderer>();
                    ColliderRenderer.enabled = false;
                    ColliderRenderer.material = Player.INSTANCE.HighlightMaterial;
                    
                    TdObjectCollider = colliderTransform.gameObject.AddComponent<MeshCollider>();
                    tdObjectFocusable = TdObjectCollider.gameObject.AddComponent<TdObjectFocusable>();
                    tdObjectFocusable.Initialize(this);
                }));
            }
        }

        private void LoadGameObject(Vector3 scale, Vector3 offset, Vector3 rotation, Vector3 initialPosition,
            float initialScale, bool detectCollision, bool reinitialize = false)
        {
            if (TdObjectCollider == null)
            {
                TdObjectCollider = tdObject.AddComponent<BoxCollider>();
                tdObjectFocusable = tdObject.AddComponent<TdObjectFocusable>();
                tdObjectFocusable.Initialize(this);
                ((BoxCollider) TdObjectCollider).center = GetRendererCenter(tdObject);
                ((BoxCollider) TdObjectCollider).size =
                    GetRendererSize(((BoxCollider) TdObjectCollider).center, tdObject);
                tdObject.transform.SetParent(tdObjectContainer.transform, false);
            }

            if (reinitialize)
            {
                tdObject.transform.localScale = Vector3.one;
                tdObject.transform.localPosition = Vector3.zero;

                var size = ((BoxCollider) TdObjectCollider).size;
                var maxD = new[] {size.x, size.y, size.z}.Max();
                initialScale = maxD > 10f ? 10f / maxD : 1;

                tdObject.transform.localScale = initialScale * Vector3.one;
                InitializeProps(
                    tdObjectContainer.transform.TransformPoint(Vector3.zero) -
                    TdObjectCollider.transform.TransformPoint(((BoxCollider) TdObjectCollider).center), initialScale);
                return;
            }

            tdObject.transform.localScale = initialScale * Vector3.one;
            tdObject.transform.localPosition = initialPosition;

            tdObjectContainer.transform.localScale = scale;
            tdObjectContainer.transform.localPosition = offset;
            tdObjectContainer.transform.eulerAngles = rotation;

            tdObject.layer =
                detectCollision ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("3DColliderOff");
            if (GetBlock().land != null && !InLand(((BoxCollider) TdObjectCollider)))
            {
                DestroyObject();
                UpdateStateAndIcon(StateMsg.OutOfBound);
            }
            else
            {
                UpdateStateAndIcon(StateMsg.Ok);
                BlockSelectionController.INSTANCE.ReCreateTdObjectHighlightIfSelected(
                    Vectors.FloorToInt(transform.position));
            }
        }

        private void DestroyObject(bool immediate = true)
        {
            if (tdObjectFocusable != null)
            {
                tdObjectFocusable.UnFocus();
                tdObjectFocusable = null;
            }

            if (tdObject != null)
            {
                foreach (var meshRenderer in tdObject.GetComponentsInChildren<MeshRenderer>())
                foreach (var mat in meshRenderer.sharedMaterials)
                {
                    if (mat == null) continue;
                    if (immediate)
                    {
                        DestroyImmediate(mat.mainTexture);
                        DestroyImmediate(mat);
                    }
                    else
                    {
                        Destroy(mat.mainTexture);
                        Destroy(mat); // TODO: not allowed to destroy GLB mats
                    }
                }

                foreach (var meshFilter in tdObject.GetComponentsInChildren<MeshFilter>())
                {
                    if (immediate)
                        DestroyImmediate(meshFilter.sharedMesh);
                    else
                        Destroy(meshFilter.sharedMesh);
                }


                if (immediate)
                    DestroyImmediate(tdObject.gameObject);
                else
                    Destroy(tdObject.gameObject);

                tdObject = null;
            }

            if (tdObjectContainer != null)
            {
                DestroyImmediate(tdObjectContainer.gameObject);
                tdObjectContainer = null;
            }

            TdObjectCollider = null;
        }

        public void InitializeProps(Vector3 initialPosition, float initialScale)
        {
            var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
            props.initialPosition = new SerializableVector3(initialPosition);
            props.initialScale = initialScale;
            GetBlock().SetProps(props, land);
        }

        public void UpdateProps()
        {
            var props = new TdObjectBlockProperties(GetBlock().GetProps() as TdObjectBlockProperties);
            if (tdObjectContainer == null) return;
            props.offset = new SerializableVector3(tdObjectContainer.transform.localPosition);
            props.rotation = new SerializableVector3(tdObjectContainer.transform.eulerAngles);
            props.scale = new SerializableVector3(tdObjectContainer.transform.localScale);
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

        private static Vector3 GetRendererSize(Vector3 center, GameObject loadedObject)
        {
            var bounds = new Bounds(center, Vector3.zero);
            foreach (var child in loadedObject.GetComponentsInChildren<MeshRenderer>())
            {
                bounds.Encapsulate(child.bounds);
            }

            return bounds.size;
        }

        private IEnumerator LoadBytes(string url, TdObjectBlockProperties.TdObjectType type,
            Action<GameObject> onSuccess)
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
                    Debug.LogError($"Get for {url} caused Error: {webRequest.error}");
                    DestroyObject();
                    UpdateStateAndIcon(StateMsg.ConnectionError);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"Get for {url} caused HTTP Error: {webRequest.error}");
                    DestroyObject();
                    UpdateStateAndIcon(StateMsg.InvalidUrlOrData);
                    break;
                case UnityWebRequest.Result.Success:
                    Action onFailure = () =>
                    {
                        DestroyObject();
                        UpdateStateAndIcon(StateMsg.InvalidData);
                    };

                    switch (type)
                    {
                        case TdObjectBlockProperties.TdObjectType.OBJ:
                            ObjLoader.INSTANCE.InitTask(webRequest.downloadHandler.data, onSuccess, onFailure);
                            break;
                        case TdObjectBlockProperties.TdObjectType.GLB:
                            GlbLoader.InitTask(webRequest.downloadHandler.data, onSuccess, onFailure);
                            break;
                        default:
                            onFailure.Invoke();
                            break;
                    }

                    break;
            }
        }

        private void OnDestroy()
        {
            DestroyObject(false);
            base.OnDestroy();
        }
    }
}