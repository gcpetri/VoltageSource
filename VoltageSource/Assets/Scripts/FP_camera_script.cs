using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FP_camera_script : MonoBehaviour
{
    public float lookSensitivity = 100f;
    public Transform PlayerBody;


    float xRotation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float MouseX = Input.GetAxis("MouseX") * lookSensitivity * Time.deltaTime;
        float MouseY = Input.GetAxis("MouseY") * lookSensitivity * Time.deltaTime;
        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        PlayerBody.Rotate(Vector3.up * MouseX);
    }
}
