using src.Canvas.Map;
using src.Utils;
using TMPro;
using UnityEngine;

namespace src.Canvas
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

        void Update()
        {
            var mousePos = Input.mousePosition;
            var mousePosInt = Vectors.FloorToInt(mousePos);
            var realPosition = Vectors.FloorToInt(mousePos - landRect.GetComponent<RectTransform>().position);
            positionText.text = string.Format("{0} {1}", realPosition.x, realPosition.y);

            if (!drawing && !dragging && Input.GetMouseButtonDown(0))
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

            //TODO open buy dialog
        }

        private void Draw(Vector3Int mousePos)
        {
            var drawingRect = drawingObject.GetComponent<RectTransform>();
            var mouseX = Round(mousePos.x);
            var mouseY = Round(mousePos.y);
            var rectPos = startDrawPos;
            float x2 = Mathf.Max(rectPos.x, mouseX);
            float x1 = Mathf.Min(rectPos.x, mouseX);
            float y2 = Mathf.Max(rectPos.y, mouseY);
            float y1 = Mathf.Min(rectPos.y, mouseY);
            var rect = new Rect(x1, y1, x2 - x1, y2 - y1);
            if (!landRect.OverlapsOthers(drawingObject, rect))
            {
                drawingRect.localPosition = new Vector3(x1, y1, 0);
                drawingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
                drawingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            }
        }

        private static Vector3Int Round(Vector3 v)
        {
            return new Vector3Int(Round(v.x), Round(v.y), Round(v.z));
        }

        private static int Round(float x)
        {
            return 5 * (int) Mathf.Floor(x / 5);
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
            startDrawPos = Round(pos);
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