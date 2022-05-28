using System;
using System.Collections;
using UnityEngine;

namespace src
{
    public class MouseLook : MonoBehaviour
    {
        public float mouseSensitivity = 1;
        public Player player;
        private float xRotation = 0f;
        private Action onUpdate = () => { };
        private Action<Vector3> rotationTarget = null;
        public bool cursorLocked = true;

        void Start()
        {
            mouseSensitivity = 180;

            if (Application.isEditor)
                mouseSensitivity = 400;

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING || state == GameManager.State.MOVING_OBJECT)
                {
                    onUpdate = DoUpdate;
                }
                else
                {
                    UnlockCursor();
                    onUpdate = () => { };
                }
            });
        }

        private void Update()
        {
            onUpdate.Invoke();

            if (cursorLocked && Input.GetButtonDown("Cancel"))
                UnlockCursor();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                UnlockCursor();
        }

        public void UnlockCursor()
        {
            StartCoroutine(ChangeCursorState(false));
        }

        public void LockCursor()
        {
            StartCoroutine(ChangeCursorState(true));
        }

        private IEnumerator ChangeCursorState(bool locked)
        {
            yield return null;
            cursorLocked = locked;
            Cursor.visible = !locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        }

        private void DoUpdate()
        {
            if (!cursorLocked) return;
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            if (Mathf.Abs(mouseX) > 20 || Mathf.Abs(mouseY) > 20)
                return;

            if (rotationTarget == null)
            {
                // camera's x rotation (look up and down)
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
                player.avatar.transform.Rotate(Vector3.up * mouseX);
            }
            else
                rotationTarget.Invoke(Vector3.up * mouseX + Vector3.right * mouseY);
        }

        public void SetRotationTarget(Action<Vector3> action)
        {
            rotationTarget = action;
        }

        public void RemoveRotationTarget()
        {
            rotationTarget = null;
        }

        private static bool MouseInScreen()
        {
            var mousePosition = Input.mousePosition;
            if (mousePosition.x <= 0 || mousePosition.x >= Screen.width - 1 ||
                mousePosition.y <= 0 || mousePosition.y >= Screen.height - 1)
                return false;
            return true;
        }

        public static MouseLook INSTANCE => GameObject.Find("Main Camera").GetComponent<MouseLook>();
    }
}