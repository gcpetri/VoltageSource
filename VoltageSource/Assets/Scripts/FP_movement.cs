using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FP_movement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    [SerializeField] private PhotonView photonView;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (photonView == null)
        {
            Debug.LogError("Photon view reference in movement is missing");
            return;
        }
        if (photonView.IsMine == false && PhotonNetwork.IsConnected)
        {
            return;
        }
        
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x) + (transform.forward * z);

        controller.Move(move * (speed * Time.deltaTime));

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
