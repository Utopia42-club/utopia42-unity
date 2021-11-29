using src.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class SelectionHandler : MonoBehaviour, IPointerClickHandler
    {
        public GameObject transformButton;
        public RectPane rectPane;
        public Land land;
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

        public void SetSelected(bool selected, bool fromParent)
        {
            this.selected = selected;
            if (selected)
            {
                rectPane.OpenDialogForLand(this);
                orgColor = image.color;
                image.color = Color.Lerp(orgColor, Color.black, .2f);
            }
            else
            {
                image.color = orgColor;
                if (!fromParent)
                    rectPane.OpenDialogForLand(null);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (land != null && !rectPane.landProfileDialog.gameObject.activeSelf && (eventData.pressPosition - eventData.position).magnitude < 0.1f)
                SetSelected(true, false);
        }
    }
}