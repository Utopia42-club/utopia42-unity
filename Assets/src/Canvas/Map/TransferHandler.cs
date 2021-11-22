using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransferHandler : MonoBehaviour, IPointerDownHandler
{
    public GameObject transformButton;
    public RectPane rectPane;
    public long landId;
    public string walletId;

    private bool selected;
    private Color orgColor;
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (landId >= 0)
            setSelected(!selected, false);
    }

    public void setSelected(bool selected, bool fromParent)
    {
        this.selected = selected;
        if (selected)
        {
            rectPane.setSelected(this);
            orgColor = image.color;
            image.color = Color.Lerp(orgColor, Color.white, .4f);
        }
        else
        {
            image.color = orgColor;
            if (!fromParent)
                rectPane.setSelected(null);
        }
    }
}
