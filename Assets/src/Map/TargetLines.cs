using UnityEngine;
using UnityEngine.UI;

public class TargetLines : MonoBehaviour
{
    [SerializeField]
    RectTransform vertical;
    [SerializeField]
    RectTransform horizontal;
    [SerializeField]
    Text positionText;
    [SerializeField]
    RectTransform landRect;
    private bool dragging = false;
    private Vector3 lastDragPos;

    void Update()
    {
        var mousePos = Input.mousePosition;
        var mousePosInt = Vectors.FloorToInt(mousePos);
        var realPosition = Vectors.FloorToInt(mousePos - landRect.position);
        positionText.text = string.Format("{0} {1}", realPosition.x, realPosition.y);

        if (!dragging && Input.GetMouseButtonDown(0))
        {
            lastDragPos = mousePosInt;
            dragging = true;
        }

        if (!Input.GetMouseButton(0)) dragging = false;

        if (dragging)
        {
            landRect.localPosition += mousePosInt - lastDragPos;
            lastDragPos = mousePosInt;
        }
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.INSTANCE.MovePlayerTo(new Vector3(realPosition.x, 0, realPosition.y));
        }

        SetLinesPos(mousePosInt);
    }

    private void SetLinesPos(Vector3 mousePos)
    {
        var vp = vertical.position;
        vertical.position = new Vector3(mousePos.x, vp.y, 0);

        var hp = horizontal.position;
        horizontal.position = new Vector3(hp.x, mousePos.y, 0);
    }
}
