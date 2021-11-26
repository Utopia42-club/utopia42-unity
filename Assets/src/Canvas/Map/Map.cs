using System;
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