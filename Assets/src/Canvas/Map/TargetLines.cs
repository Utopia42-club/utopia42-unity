using src.Utils;
using TMPro;
using UnityEngine;

namespace src.Canvas.Map
{
    public class TargetLines : MonoBehaviour
    {
        [SerializeField] RectTransform vertical;
        [SerializeField] RectTransform horizontal;
        [SerializeField] TextMeshProUGUI positionText;
        [SerializeField] RectPane landRect;
        private bool dragging = false;
        private Vector3 lastDragPos;
        private Vector3Int startDrawPos;
        private bool drawing = false;
        private GameObject drawingObject;
        public Map map;

        void Update()
        {
            var mousePos = Input.mousePosition;
            var mousePosInt = Vectors.FloorToInt(mousePos);
            var realPosition = Vectors.FloorToInt(mousePos - landRect.GetComponent<RectTransform>().position);
            positionText.text = $"{realPosition.x} {realPosition.y}";

            if (!drawing && !dragging && Input.GetMouseButtonDown(0) &&
                !landRect.landProfileDialog.gameObject.activeSelf)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                                                      || Input.GetKey(KeyCode.LeftCommand) ||
                                                      Input.GetKey(KeyCode.RightCommand))
                    StartDraw(realPosition);
                else
                    StartDrag(mousePosInt);
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

            if (Input.GetMouseButtonDown(1))
                GameManager.INSTANCE.MovePlayerTo(new Vector3(realPosition.x, 0, realPosition.y));

            SetLinesPos(mousePosInt);
        }

        private void FinishDraw()
        {
            drawing = false;

            var drawingRect = drawingObject.GetComponent<RectTransform>();
            var rect = drawingRect.rect;
            if ((long) rect.xMin == (long) rect.xMax || (long) rect.yMin == (long) rect.yMax)
                landRect.Delete(drawingObject);

            map.OpenLandBuyDialogState(drawingRect, () => { landRect.Delete(drawingObject); });
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
            var or = drawingRect.rect;
            var olp = drawingRect.localPosition;
            int ox1 = (int) olp.x;
            int ox2 = ox1 + (int) or.width;
            int oy1 = (int) olp.y;
            int oy2 = oy1 + (int) or.height;

            // Debug.Log($"{ox1},{ox2},{oy1},{oy2}, {x1},{x2},{y1},{y2}");
            var rect = landRect.ResolveCollisions(drawingObject, ox1, ox2, oy1, oy2, x1 - ox1, x2 - ox2,
                y1 - oy1, y2 - oy2);

            drawingRect.localPosition = new Vector3(rect.x, rect.y, 0);
            drawingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
            drawingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
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
    }
}