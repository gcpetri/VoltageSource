using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FP_camera_script : MonoBehaviour
{
    public float lookSensitivity = 100f;
    public Transform PlayerBody;


    float _xRotation = 0f;
    // Start is called before the first frame update
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void Update()
    {
        float mouseX = Input.GetAxis("MouseX") * (lookSensitivity * Time.deltaTime);
        float mouseY = Input.GetAxis("MouseY") * (lookSensitivity * Time.deltaTime);
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        PlayerBody.Rotate(Vector3.up * mouseX);
    }
}
