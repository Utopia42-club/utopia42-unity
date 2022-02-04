using System;
using UnityEngine;

namespace src
{
    public class MouseLook : MonoBehaviour
    {
        public float mouseSensitivity = 1;
        public Transform playerBody;
        private float xRotation = 0f;
        private Action onUpdate = () => { };
        private Action<Vector3> rotationTarget = null;

        void Start()
        {
            mouseSensitivity = 180;

            if (Application.isEditor)
                mouseSensitivity = 400;

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING || state == GameManager.State.MOVING_OBJECT)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    this.onUpdate = DoUpdate;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    this.onUpdate = () => { };
                }
            });
        }

        void Update()
        {
            onUpdate.Invoke();
        }

        private void DoUpdate()
        {
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
                playerBody.Rotate(Vector3.up * mouseX);
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

        public static MouseLook INSTANCE
        {
            get { return GameObject.Find("Main Camera").GetComponent<MouseLook>(); }
        }
    }
}