using System;
using System.Collections;
using Source.Model;
using Source.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Source.Canvas.Map
{
    public class Map : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private LandBuyDialog landBuyDialog;
        private Action landBuyDialogDismissCallback;

        void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(
                state =>
                {
                    gameObject.SetActive(state == GameManager.State.MAP);
                    CloseLandBuyDialogState();
                }
            );
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (LandProfileDialog.INSTANCE.gameObject.activeSelf
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

        public IEnumerator TakeNftScreenShot(Land land, Action<byte[]> consumer)
        {
            var mapInputManager = GameObject.Find("InputManager").GetComponent<MapInputManager>();
            mapInputManager.PrepareForScreenShot(land);
            LandProfileDialog.INSTANCE.CloseIfOpened();
            yield return new WaitForEndOfFrame();

            // var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            var width = Screen.width;
            var height = Screen.height;
            var screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            yield return null;

            consumer.Invoke(screenshot.EncodeToPNG());
            mapInputManager.ScreenShotDone();
            Destroy(screenshot);
        }


        public bool IsLandBuyDialogOpen()
        {
            return landBuyDialog.gameObject.activeInHierarchy;
        }

        public bool IsLandProfileDialogOpen()
        {
            return LandProfileDialog.INSTANCE.gameObject.activeSelf;
        }

        public void OpenLandBuyDialogState(RectTransform rect, Action dismissCallback)
        {
            landBuyDialog.gameObject.SetActive(true);
            landBuyDialog.SetRect(rect);
            landBuyDialogDismissCallback = dismissCallback;
            if (TabMenu.INSTANCE != null)
                TabMenu.INSTANCE.SetActionsEnabled(false);
        }

        public void CloseLandBuyDialogState()
        {
            landBuyDialog.gameObject.SetActive(false);
            landBuyDialogDismissCallback?.Invoke();
            if (TabMenu.INSTANCE != null)
                TabMenu.INSTANCE.SetActionsEnabled(true);
        }
    }
}