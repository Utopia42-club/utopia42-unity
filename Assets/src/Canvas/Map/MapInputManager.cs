using System.Linq;
using src.Model;
using src.Utils;
using TMPro;
using UnityEngine;

namespace src.Canvas.Map
{
    public class MapInputManager : MonoBehaviour
    {
        [SerializeField] RectTransform vertical;
        [SerializeField] RectTransform horizontal;
        [SerializeField] RectPane landRect;
        [SerializeField] GameObject sidePanel;
        [SerializeField] GameObject overlayPrefab;
        [SerializeField] GameObject helpMessage;
        [SerializeField] GameObject positionBox;

        private bool dragging = false;
        private bool scrollLock = false;
        private Vector3 lastDragPos;
        private Vector3Int startDrawPos;
        private bool drawing = false;
        private GameObject drawingObject;
        public Map map;
        private GameManager gameManager;
        private GameObject screenShotOverlay;
        private TextMeshProUGUI positionText;

        private const float MoveSpeed = 5f;
        private const float BoostedMoveSpeed = 15f;

        private const float MaxZoomOutScale = 0.25f;

        private void Start()
        {
            gameManager = GameManager.INSTANCE;
            positionText = positionBox.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (sidePanel.activeSelf)
                ToggleSidePanel();
        }

        void Update()
        {
            var mousePos = Input.mousePosition;
            var mousePosInt = Vectors.FloorToInt(mousePos);
            var mouseLocalPos = ScreenToLandContainerLocal(mousePos);
            var realPosition = Vectors.FloorToInt(mouseLocalPos);
            positionText.text = $"({realPosition.x}, {realPosition.y})";

            if (Input.GetButtonDown("Menu"))
            {
                ToggleSidePanel();
            }

            else if (IsInputEnabled())
            {
                if (!drawing && !dragging)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                                                              || Input.GetKey(KeyCode.LeftCommand)
                                                              || Input.GetKey(KeyCode.RightCommand))
                            StartDraw(realPosition);
                        else
                            StartDrag(mousePosInt);
                    }

                    if (!scrollLock && Input.mouseScrollDelta.y != 0)
                    {
                        var multiplier = Input.mouseScrollDelta.y < 0 ? (float) 0.5 : 2;
                        var scale = landRect.landContainer.localScale.x * multiplier;
                        scale = Mathf.Min(4f, scale);
                        // Center the map to mouse position 
                        ScaleMap(Mathf.Max(0.25f, scale));
                    }
                }

                if (!Input.GetMouseButton(0))
                {
                    if (drawing) FinishDraw();
                    dragging = false;
                }

                if (dragging)
                    Drag(mousePosInt);
                else if (drawing)
                    Draw(realPosition);

                HandleKeyboardInput();
            }

            SetLinesPos(mousePosInt);
        }

        public void ToggleSidePanel()
        {
            sidePanel.SetActive(!sidePanel.activeSelf);
            if (!sidePanel.activeSelf)
                UnLockScroll();
        }

        public void LockScroll()
        {
            scrollLock = true;
        }

        public void UnLockScroll()
        {
            scrollLock = false;
        }

        private bool IsInputEnabled()
        {
            return !map.IsLandBuyDialogOpen() && !map.IsLandProfileDialogOpen();
        }

        public Vector3 ScreenToLandContainerLocal(Vector3 pos)
        {
            var landRectTransform = landRect.GetComponent<RectTransform>();
            var local = pos - landRectTransform.position;
            var landContainerScale = landRect.landContainer.localScale;
            local.Scale(new Vector3(1 / landContainerScale.x, 1 / landContainerScale.y,
                1 / landContainerScale.z));
            return local;
        }

        private void ScaleMap(float scale)
        {
            var preScale = landRect.landContainer.localScale.x;
            landRect.landContainer.localScale = new Vector3(scale, scale, 1);
            landRect.GetComponent<RectTransform>().localPosition =
                scale / preScale * landRect.GetComponent<RectTransform>().localPosition;
        }

        private void FinishDraw()
        {
            drawing = false;

            var drawingRect = drawingObject.GetComponent<RectTransform>();
            var rect = drawingRect.rect;
            if ((long) rect.xMin == (long) rect.xMax || (long) rect.yMin == (long) rect.yMax)
                landRect.DeleteDrawingObject(drawingObject);
            else
                map.OpenLandBuyDialogState(drawingRect, () => { landRect.DeleteDrawingObject(drawingObject); });
        }

        private void Draw(Vector3Int mousePos)
        {
            var drawingRect = drawingObject.GetComponent<RectTransform>();
            var mouseX = RoundDown(mousePos.x);
            var mouseY = RoundDown(mousePos.y);
            var rectPos = startDrawPos;

            // New Rect
            int x2 = Mathf.Max(rectPos.x, mouseX);
            int x1 = Mathf.Min(rectPos.x, mouseX);
            int y1 = Mathf.Min(rectPos.y, mouseY);
            int y2 = Mathf.Max(rectPos.y, mouseY);

            // Old Rect
            var oldRect = drawingRect.rect;
            var oldPosition = drawingRect.localPosition;
            int ox1 = (int) oldPosition.x;
            int ox2 = ox1 + (int) oldRect.width;
            int oy1 = (int) oldPosition.y;
            int oy2 = oy1 + (int) oldRect.height;

            // Debug.Log($"{ox1},{ox2},{oy1},{oy2}, {x1},{x2},{y1},{y2}");
            var rect = landRect.ResolveCollisions(drawingObject, ox1, ox2, oy1, oy2, x1 - ox1, x2 - ox2,
                y1 - oy1, y2 - oy2);

            drawingRect.localPosition = new Vector3(rect.x, rect.y, 0);
            drawingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
            drawingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            drawingRect.localScale = Vector3.one;
        }

        private static Vector3Int RoundDown(Vector3 v)
        {
            return new Vector3Int(RoundDown(v.x), RoundDown(v.y), RoundDown(v.z));
        }

        public static int RoundDown(float x)
        {
            return 5 * (int) Mathf.Floor(x / 5);
        }

        public static int RoundUp(float x)
        {
            return 5 * (int) Mathf.Ceil(x / 5);
        }

        private void StartDrag(Vector3Int mousePosInt)
        {
            dragging = true;
            lastDragPos = mousePosInt;
        }

        private void Drag(Vector3Int mousePosInt)
        {
            landRect.GetComponent<RectTransform>().localPosition += mousePosInt - lastDragPos;
            lastDragPos = mousePosInt;
        }

        private void StartDraw(Vector3Int pos)
        {
            startDrawPos = RoundDown(pos);
            drawing = true;
            drawingObject = landRect.DrawAt(pos.x, pos.y);
        }

        private void SetLinesPos(Vector3 mousePos)
        {
            var vp = vertical.position;
            vertical.position = new Vector3(mousePos.x, vp.y, 0);

            var hp = horizontal.position;
            horizontal.position = new Vector3(hp.x, mousePos.y, 0);
        }

        private void HandleKeyboardInput()
        {
            var boosted = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                Move(Vector3.right, boosted);
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                Move(Vector3.left, boosted);
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                Move(Vector3.up, boosted);
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                Move(Vector3.down, boosted);
        }

        private void Move(Vector3 direction, bool boosted)
        {
            landRect.GetComponent<RectTransform>().localPosition -=
                direction * (boosted ? BoostedMoveSpeed : MoveSpeed);
        }

        public void PrepareForScreenShot(Land land)
        {
            ZoomOutOnLand(land);
            MoveToLandCenter(land);
            vertical.gameObject.SetActive(false);
            horizontal.gameObject.SetActive(false);
            positionText.gameObject.SetActive(false);
            landRect.HidePlayerPosIndicator();
            screenShotOverlay = Instantiate(overlayPrefab, landRect.landContainer.transform);
            var rectSize = map.gameObject.GetComponent<RectTransform>().rect.size;
            rectSize = rectSize * 1 / landRect.landContainer.localScale;
            screenShotOverlay.GetComponentInChildren<RectTransform>().sizeDelta = rectSize;
            screenShotOverlay.transform.position = map.transform.position;
            landRect.MoveLandGameObjectToFront(land);
            helpMessage.SetActive(false);
            if (sidePanel.activeSelf)
                ToggleSidePanel();
        }

        public void ScreenShotDone()
        {
            vertical.gameObject.SetActive(true);
            horizontal.gameObject.SetActive(true);
            positionText.gameObject.SetActive(true);
            helpMessage.SetActive(true);
            landRect.ShowPlayerPosIndicator();
            DestroyImmediate(screenShotOverlay);
        }

        private void MoveToLandCenter(Land land)
        {
            var c = ((Vector3) (land.startCoordinate.ToVector3() + land.endCoordinate.ToVector3())) / 2;
            var landCenter = new Vector3(c.x, c.z, 0);
            var landContainerScale = landRect.landContainer.localScale;
            landCenter.Scale(new Vector3(landContainerScale.x, landContainerScale.y, 0));
            landRect.GetComponent<RectTransform>().localPosition = -landCenter;
        }

        private void ZoomOutOnLand(Land land)
        {
            var rect = land.ToRect();

            var widthRatio = Screen.width / (rect.width * 1.5);
            var heightRatio = Screen.height / (rect.height * 1.5);

            var scale = (float) new[] {widthRatio, heightRatio, MaxZoomOutScale}.Min();
            landRect.landContainer.localScale = new Vector3(scale, scale, 1);
        }

        public void NavigateInMap(Land land)
        {
            landRect.SetTargetLand(land);
            MoveToLandCenter(land);
        }
    }
}