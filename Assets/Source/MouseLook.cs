using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Source
{
    public class MouseLook : MonoBehaviour
    {
        public float mouseSensitivity = 1;
        private float xRotation = 0f;
        private float yRotation = 0f;
        private Action onUpdate = () => { };
        private Action<Vector3> rotationTarget = null;
        public bool cursorLocked = true;
        public readonly UnityEvent<bool> cursorLockedStateChanged = new();

        void Start()
        {
            mouseSensitivity = 180;

            if (Application.isEditor)
                mouseSensitivity = 400;

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING)
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
            cursorLockedStateChanged.Invoke(locked);
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
                xRotation -= mouseY; // camera's x rotation (look up and down)
                var limit = Player.INSTANCE.GetViewMode() == Player.ViewMode.FIRST_PERSON ? 90f : 45f;
                xRotation = Mathf.Clamp(xRotation, -limit, limit);
                yRotation += mouseX; // camera's y rotation (look left and right)
                transform.parent.localRotation = Quaternion.Euler(xRotation, yRotation % 360, 0);
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