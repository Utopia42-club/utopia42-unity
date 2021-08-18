using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ClickableLink : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMeshProUGUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        int index = TMP_TextUtilities.FindIntersectingLink(textMeshProUGUI, Input.mousePosition, null);
        if (index > -1)
        {
            TMP_LinkInfo linkInfo = textMeshProUGUI.textInfo.linkInfo[index];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
