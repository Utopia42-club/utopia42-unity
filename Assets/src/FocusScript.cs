using src;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusScript : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
            MouseLook.INSTANCE.LockCursor();
    }
}