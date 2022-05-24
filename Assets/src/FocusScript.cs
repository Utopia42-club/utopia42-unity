using src;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusScript : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
        if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
            MouseLook.INSTANCE.LockCursor();
    }
}