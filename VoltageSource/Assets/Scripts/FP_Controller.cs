#define DEBUG_VARIABLES
#define DEBUG_METHODS

using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using VoltageSource;



// Require component tells unity when you add this script to a game object
// that it must also add these components as well as they are required for the
// script to run
[RequireComponent(typeof(PhotonView), 
    typeof(Rigidbody), 
    typeof(CharacterController))]
public class FP_Controller : MonoBehaviourPunCallbacks, IPunObservable
{
    
    #region Networking Variables

    private PhotonView _photonView;
    [SerializeField] private AudioListener localAudioListener; // Reference

    #endregion
    
    #region Character Movement Variables
    // Contains all the variables used from FP_movement 
    [Header("Character Movement Variables")]
    private CharacterController _cController; // Reference
    [Tooltip("Controls the speed at which the player moves")]
    [SerializeField] private float playerSpeed = 12f; // Local
    [Tooltip("The height at which the player can jump")]
    [SerializeField] private float jumpHeight = 3f; // Local
    [Tooltip("Layer mask for what is considered ground to check if player is grounded")]
    [SerializeField] private LayerMask groundMask; // Local

    private Vector3 _velocity; // Local
    private bool _isGrounded; // Local
    private Vector3 _groundCheckPosition = Vector3.zero;
    #endregion

    #region Player Health Varaibles

    // Handle being hit and store health. Best done by get and set funcitons
        [SerializeField] private float health;
        public float Health
        {
            get => health;
            set
            {
                //Debug.LogErrorFormat("PhotonView {0} took damage, current health is {1}", photonView.ViewID, health);
                if (health <= 0)
                {
                    Debug.LogFormat("Player with photon ID: {0} died", photonView.ViewID);
                    // If player dies then all function to handle their death
                    // Call GameManager 
                }
                health = value;
            }
        }

    #endregion

    #region Camera Variables

    [Header("Camera Variables")] [SerializeField]
    private float lookSensitivity = 100f; // Local

    private float _xRotation = 0f; // Local
    public Camera fpsCamera; // Reference

    #endregion

    #region Gun Variables

    [Header("Gun Variables")] [SerializeField]
    private GameObject currentGun; // Reference
    float _nextTimeToFire = 0f;
    private float _fireRate = 1f;
    private GunScript _currentGunInfo; // Reference
    [SerializeField] GameObject gunRotationPoint;
    #endregion

    #region Animator Variables

    [Header("Animator variables")] [SerializeField]
    private Animator anim; // Reference 

    private AnimatorControllerParameter[] _animParams; // Copied 

    #endregion

    #region UI Variables

    [SerializeField] private GameObject uiPrefab; // Reference 
    

    #endregion
    
    /// <summary>
    /// Initialize all the values we need to run this script
    /// </summary>
    void Start()
    {
        _photonView = GetComponent<PhotonView>(); // Need to get this value to do networking

        _cController = GetComponent<CharacterController>();
        _currentGunInfo = currentGun.GetComponent<GunScript>();
        if (photonView.IsMine)
        {
            fpsCamera.enabled = true;
            localAudioListener.enabled = true;
        }

        if (uiPrefab != null)
        {
            GameObject uiGo = Instantiate(uiPrefab);
            uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("Missing uiPrefab reference on player prefab");
        }

        if (anim != null)
        {
            _animParams = anim.parameters;
        }

        if (_currentGunInfo != null)
        {
            _currentGunInfo.SetOwner(this);
        }
        
        _fireRate = _currentGunInfo.GetFireRate();
        _groundCheckPosition = transform.position - (new Vector3(0, _cController.height / 2, 0));
        
        var stack = fpsCamera.GetUniversalAdditionalCameraData();
        stack.cameraStack.Add(_currentGunInfo.GetCamera().GetComponent<Camera>());
        
        /*
        #if DEBUG_VARIABLES
         // Checks to see if all required dependicies exists
         Debug.LogFormat("PhotonView exists: {0} on {1}", photonView != null, gameObject.name);
         Debug.LogFormat("CharacterController exists: {0} on {1}", (_cController != null), gameObject.name);
         Debug.LogFormat("CurrentGun exists: {0} on {1}", (currentGun != null), gameObject.name);
         Debug.LogFormat("Animator exists: {0} on {1}", (anim != null), gameObject.name);
         Debug.LogFormat("Camera exists: {0} on {1}", (fpsCamera != null), gameObject.name);
         Debug.LogFormat("LocalAudioListener exists: {0} on {1}", (localAudioListener != null), gameObject.name);
         Debug.LogFormat("CurrentGunScript exists: {0} on {1}", (_currentGunInfo != null), gameObject.name);
        #endif
        */
    }
    
    // Update is called once per frame
    void Update()
    {
        
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;


        if (Input.GetButton("Fire2"))
            _currentGunInfo.isAiming = true;
        else
            _currentGunInfo.isAiming = false;


        if (Input.GetButton("Fire1"))
        {
            // The if statement is used just to reduce number of RPC calls between clients
            if (_nextTimeToFire < Time.time)
                photonView.RPC("ShootRPC", RpcTarget.All, null);

        }

        GroundCheck();
        InputProcess();
        
        _cController.Move(_velocity * Time.deltaTime);
        
    }

    private void FixedUpdate()
    {
        // Update groundcheck location based on recent transform.position
        // Placed in FixedUpdate since we only want it to update every 60 ticks per second so its not based on fps
        _groundCheckPosition = transform.position - (new Vector3(0, _cController.height / 2, 0)); 
        _velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Processes all the inputs required for the FP_Controller
    /// </summary>
    private void InputProcess()
    {
        var xAxis = Input.GetAxis("Horizontal");
        var zAxis = Input.GetAxis("Vertical");
        var mouseX = Input.GetAxis("Mouse X") * (lookSensitivity * Time.deltaTime);
        var mouseY = Input.GetAxis("Mouse Y") * (lookSensitivity * Time.deltaTime);

        MovePlayer(xAxis, zAxis); // Moves the player
        MoveCamera(mouseX, mouseY); // Moves the camera

        if (xAxis != 0 || zAxis != 0)
            anim.SetBool(_animParams[0].name, true);
        else
            anim.SetBool(_animParams[0].name, false);

        if (Input.GetButtonDown("Jump") && _isGrounded) // Checks if jumps and runs appropriate code
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            anim.SetTrigger(_animParams[3].name);
            #if  DEBUG_METHODS
                Debug.Log("Jump button was pressed");
            #endif
        }
    }

    #region Movement/Camera Methods

        /// <summary>
        /// Performs the check to see if the FP_Controller is grounded
        /// </summary>
        private void GroundCheck()
        {
            _isGrounded = Physics.CheckSphere(_groundCheckPosition, _cController.radius, groundMask);
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }
    
        #if DEBUG_METHODS
        private void OnDrawGizmos()
        {
            if (_cController == null)
                return;
            Gizmos.DrawSphere(_groundCheckPosition,  _cController.radius);
        }
        #endif
    
        private void MovePlayer(float xMovement, float zMovement)
        {
            var thisTransfrom = transform; // This is done for efficiency instead of calling transform on this object
            Vector3 moveDirection = (thisTransfrom.right * xMovement) + (thisTransfrom.forward * zMovement);
            _cController.Move(moveDirection * (playerSpeed * Time.deltaTime));
        }
    
        private void MoveCamera(float xMove, float yMove)
        {
            _xRotation = Mathf.Clamp(_xRotation - yMove, -90f, 90f);
            fpsCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * xMove);
            gunRotationPoint.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            gunRotationPoint.transform.Rotate(Vector3.up * xMove);
        }

    #endregion

    [PunRPC]
    private void ShootRPC()
    {
        #if DEBUG_METHODS
            Debug.LogFormat("ShootRPC method called by photon id: {0}", photonView.ViewID);
        #endif
        _currentGunInfo.Shoot(photonView.ViewID);
        _nextTimeToFire = Time.time + (1 / _fireRate);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Health);
        }
        else
        {
            this.Health = (float) stream.ReceiveNext();
        }
    }
    
    
    
}
