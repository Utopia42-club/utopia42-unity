using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 1;

    public Transform playerBody;

    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mouseSensitivity = 180;

        if (Application.isEditor)
            mouseSensitivity = 400;
    }

    float mx;

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (Mathf.Abs(mouseX) > 20 || Mathf.Abs(mouseY) > 20)
            return;

        //camera's x rotation (look up and down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void FixedUpdate()
    {
        //float mouseX = mx * mouseSensitivity * Time.fixedDeltaTime;

        // playerBody.GetComponent<Rigidbody>().MoveRotation(playerBody.GetComponent<Rigidbody>().rotation *= Quaternion.Euler(0, mouseX, 0));
    }
}
