#define DEBUG_VARIABLES
#define DEBUG_METHODS

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VoltageSource;

// Require component tells unity when you add this script to a game object
// that it must also add these components as well as they are required for the
// script to run
[RequireComponent(typeof(PhotonView), 
    typeof(Rigidbody), 
    typeof(CharacterController))]
public class FpController : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    
    #region Networking Variables

    private PhotonView _photonView;
    private bool _isPreRound = false;
    

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

        private bool _isDead = false;

    // Handle being hit and store health. Best done by get and set funcitons
        [SerializeField] private float health;
        [SerializeField]private float _maxHealth;
        public float Health
        {
            get => health;
            set
            {
                if (!photonView.IsMine)
                    return;
                //Debug.LogErrorFormat("PhotonView {0} took damage, current health is {1}", photonView.ViewID, health);
                if (health <= 0)
                {
                    // If player dies then all function to handle their death
                    // Call GameManager 
                    if (_isDead)
                        return;
                    
                    object[] content = {photonView.ViewID}; 
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.PlayerDied, content, raiseEventOptions, SendOptions.SendReliable);
                    SetDeath();
                }
                health = value;
            }
        }

    #endregion

    #region Camera Variables

    [Header("Camera Variables")] [SerializeField]
    private float lookSensitivity = 100f; // Local

    private float _xRotation; // Local
    public Camera fpsCamera; // Reference
    #endregion

    #region Gun Variables

    [Header("Gun Variables")] [SerializeField]
    private GameObject currentGun; // Reference
    private float _nextTimeToFire;
    private float _fireRate = 1f;
    private GunScript _currentGunInfo; // Reference
    [SerializeField] private GameObject gunRotationPoint;
    #endregion

    #region Animator Variables

    [Header("Animator variables")] [SerializeField]
    private Animator anim; // Reference 

    private AnimatorControllerParameter[] _animParams; // Copied 

    #endregion

    #region UI Variables

    [SerializeField] private GameObject UiGameObject; // Reference 
    private PhotonPlayerUI playerUI;
    private bool isPaused = false;

    #endregion

    private Rigidbody _rb;
    [SerializeField] private AudioListener localAudioListener; // Reference
    /// <summary>
    /// Initialize all the values we need to run this script
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _photonView = GetComponent<PhotonView>(); // Need to get this value to do networking
        _cController = GetComponent<CharacterController>();
        _currentGunInfo = currentGun.GetComponent<GunScript>();
        if (photonView.IsMine)
        {
            fpsCamera.enabled = true;
            localAudioListener.enabled = true;
            UiGameObject.SetActive(true);
        }

        _rb = GetComponent<Rigidbody>();
        Health = _maxHealth;
        
        if (UiGameObject != null && photonView.IsMine)
        {
            UiGameObject.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            playerUI = UiGameObject.GetComponent<PhotonPlayerUI>();
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
    void PauseGame()
    {
        isPaused = playerUI.TogglePause();
    }
    // Update is called once per frame
    private void Update()
    {
        
        if (!photonView.IsMine || _isDead)
            return;

        _currentGunInfo.isAiming = Input.GetButton("Fire2");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

        if (isPaused)
            return;
        
        InputProcess();

        if (_isPreRound)
            return;
        
        if (Input.GetButton("Fire1"))
        {
            // The if statement is used just to reduce number of RPC calls between clients
            if (_nextTimeToFire < Time.time)
            {
                photonView.RPC("ShootRPC", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
            }
                

        }

        GroundCheck();
        
        _cController.Move(_velocity * Time.deltaTime); // This calls to move the character downward based on gravity
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
        
        MoveCamera(mouseX, mouseY); // Moves the camera
        if (_isPreRound)
            return;
        
        MovePlayer(xAxis, zAxis); // Moves the player

        if (xAxis != 0 || zAxis != 0)
            anim.SetBool(_animParams[0].name, true);
        else
            anim.SetBool(_animParams[0].name, false);

        if (Input.GetButtonDown("Jump") && _isGrounded) // Checks if jumps and runs appropriate code
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            anim.SetTrigger(_animParams[1].name);
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
    private void ShootRPC(PhotonMessageInfo info)
    {
        _currentGunInfo.Shoot(info.photonView.ViewID);
        _nextTimeToFire = Time.time + (1 / _fireRate);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(Health);
            stream.SendNext(_rb.position);
            stream.SendNext(_rb.rotation);
            stream.SendNext(_rb.velocity);
        }
        else
        {
            this.Health = (float) stream.ReceiveNext();
            _rb.position = (Vector3) stream.ReceiveNext();
            _rb.rotation = (Quaternion) stream.ReceiveNext();
            _rb.velocity = (Vector3) stream.ReceiveNext();

            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            _rb.position += _rb.velocity * lag;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)EventManager.EventCodes.StartPreRound)
        {
            _isPreRound = true;
        }else if (eventCode == (byte) EventManager.EventCodes.EndPreRound)
        {
            _isPreRound = false;
        }
    }
    
    private void SetDeath()
    {
        anim.SetBool(_animParams[3].name, true);
        _isDead = true;
    }

    public void ResetHealth()
    {
        Health = _maxHealth;
        _isDead = false;
    }

    public void LoseHealth()
    {
        Health -= 10;
    }

    public void SetPos(Transform pos)
    {
        /*
        transform.position = pos.position;
        transform.rotation = pos.rotation;
        */

        _rb.position = pos.position;
        _rb.rotation = pos.rotation;
    }
    

}
