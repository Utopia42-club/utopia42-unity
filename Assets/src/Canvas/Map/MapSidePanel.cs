using System;
using src.Canvas;
using src.Canvas.Map;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapSidePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MapInputManager mapInputManager;
    public ActionButton close;

    private void Start()
    {
        close.AddListener(() => mapInputManager.ToggleSidePanel());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mapInputManager.LockScroll();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mapInputManager.UnLockScroll();
    }
}