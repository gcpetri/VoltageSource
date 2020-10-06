using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FP_camera_script : MonoBehaviour
{
    public float lookSensitivity = 100f;
    public Transform PlayerBody;


    [SerializeField] private PhotonView photonView;
    float _xRotation = 0f;
    // Start is called before the first frame update
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void Update()
    {
        if (photonView == null)
        {
            Debug.LogError("Photon view reference missing in camera script");
            return;
        }

        if (photonView.IsMine)
        {
            
        }
        
        float mouseX = Input.GetAxis("Mouse X"); 
        mouseX *= (lookSensitivity * Time.deltaTime);
        float mouseY = Input.GetAxis("Mouse Y");
        mouseY *= (lookSensitivity * Time.deltaTime);
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        PlayerBody.Rotate(Vector3.up * mouseX);
    }
}
