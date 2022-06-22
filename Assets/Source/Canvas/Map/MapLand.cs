using Source.Model;
using Source.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Source.Canvas.Map
{
    public class MapLand : MonoBehaviour, IPointerClickHandler, IPointerUpHandler
    {
        public Land land;

        private void OpenLandDialog()
        {
            var landProfileDialog = LandProfileDialog.INSTANCE;
            landProfileDialog.Open(land, Profile.LOADING_PROFILE);
            landProfileDialog.WithOneClose(RefreshView);
            ProfileLoader.INSTANCE.load(land.owner, landProfileDialog.SetProfile,
                () => landProfileDialog.SetProfile(Profile.FAILED_TO_LOAD_PROFILE));
        }

        private void RefreshView()
        {
            GetComponent<Image>().color = Colors.GetLandColor(land) ?? Colors.MAP_DEFAULT_LAND_COLOR;
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
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (land == null || LandProfileDialog.INSTANCE.gameObject.activeSelf) return;
            if (eventData.button == PointerEventData.InputButton.Right)
                OpenLandDialog();
        }
    }
}