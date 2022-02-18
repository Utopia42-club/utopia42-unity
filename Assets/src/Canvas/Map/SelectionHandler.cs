using src.Model;
using src.Utils;
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
        private Outline outline;

        void Start()
        {
            outline = GetComponent<Outline>();
        }
        
        public void SetSelected(bool selected, bool fromParent)
        {
            this.selected = selected;
            if (selected)
            {
                rectPane.OpenDialogForLand(this);
                orgColor = outline.effectColor;
                outline.effectColor = Color.Lerp(orgColor, Color.black, .2f);
            }
            else
            {
                outline.effectColor = orgColor;
                if (!fromParent)
                    rectPane.OpenDialogForLand(null);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (land == null
                || LandProfileDialog.INSTANCE.gameObject.activeSelf
                || !((eventData.pressPosition - eventData.position).magnitude < 0.1f))
                return;

            if (eventData.clickCount == 2)
            {
                var mousePos = Input.mousePosition;
                var mapInputManager = GameObject.Find("InputManager").GetComponent<MapInputManager>();
                var mouseLocalPos = mapInputManager.ScreenToLandContainerLocal(mousePos);
                var realPosition = Vectors.FloorToInt(mouseLocalPos);
                GameManager.INSTANCE.MovePlayerTo(new Vector3(realPosition.x, 0, realPosition.y));
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                SetSelected(true, false);
            }
        }
    }
}