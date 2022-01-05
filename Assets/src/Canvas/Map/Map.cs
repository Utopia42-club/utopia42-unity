using System;
using System.Collections;
using src.Model;
using UnityEngine;

namespace src.Canvas.Map
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private LandProfileDialog landProfileDialog;
        [SerializeField] private LandBuyDialog landBuyDialog;
        private Action landBuyDialogDismissCallback;

        void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(
                state =>
                {
                    gameObject.SetActive(state == GameManager.State.MAP);
                    CloseLandBuyDialogState();
                    CloseLandProfileDialogState();
                }
            );
        }
        
        public IEnumerator TakeNftScreenShot(Land land, Action<byte[]> consumer)
        {
            
            var mapInputManager = GameObject.Find("InputManager").GetComponent<MapInputManager>();
            mapInputManager.PrepareForScreenShot(land);
            landProfileDialog.gameObject.SetActive(false);
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
        }


        public bool IsLandBuyDialogOpen()
        {
            return landBuyDialog.gameObject.activeInHierarchy;
        }

        public bool IsLandProfileDialogOpen()
        {
            return landProfileDialog.gameObject.activeInHierarchy;
        }

        public void CloseLandProfileDialogState()
        {
            landProfileDialog.RequestClose();
            landProfileDialog.gameObject.SetActive(false);
        }

        public void OpenLandBuyDialogState(RectTransform rect, Action dismissCallback)
        {
            landBuyDialog.gameObject.SetActive(true);
            landBuyDialog.SetRect(rect);
            landBuyDialogDismissCallback = dismissCallback;
        }

        public void CloseLandBuyDialogState()
        {
            landBuyDialog.gameObject.SetActive(false);
            landBuyDialogDismissCallback?.Invoke();
        }
    }
}