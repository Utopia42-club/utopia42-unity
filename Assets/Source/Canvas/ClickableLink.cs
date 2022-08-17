using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Source.Canvas
{
    public class ClickableLink : MonoBehaviour, IPointerClickHandler
    {
        private TextMeshProUGUI textMeshProUGUI;

        // Start is called before the first frame update
        private void Start()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var index = TMP_TextUtilities.FindIntersectingLink(textMeshProUGUI, Input.mousePosition, null);
            if (index > -1)
            {
                var linkInfo = textMeshProUGUI.textInfo.linkInfo[index];
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }
    }
}