#define DEBUG_VARIABLES
#define DEBUG_METHODS

using System;
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
    public float gravityConst;
    #endregion

    #region Player Health Varaibles

        private bool _isDead = false;

    // Handle being hit and store health. Best done by get and set funcitons
        [SerializeField] private float health;
        public float _maxHealth;
        public float Health
        {
            get => health;
            set
            {
                if (!photonView.IsMine)
                    return;
                //Debug.LogErrorFormat("PhotonView {0} took damage, current health is {1}", photonView.ViewID, health);
                health = value;
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
    private GunsScriptable _currentGunScriptable;
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
    [SerializeField] public GameObject[] UIGamePauseMenuGuns;

    #endregion

    [SerializeField] private Renderer playerRenderer;

    private Rigidbody _rb;
    [SerializeField] private AudioListener localAudioListener; // Reference
    private Transform teleportLocation;

    #region GunPickup Variables

    [Header("Gun Pickup Variables")] [SerializeField]
    private LayerMask pickupMask;
    [SerializeField] private float pickupDistance = 5f;
    private RaycastHit _pickUpHit;
    [SerializeField] private GameObject pistolRef;
    [SerializeField] private GameObject sniperRef;
    [SerializeField] private GameObject assualtRef;
    [SerializeField] private GameObject shotgunRef;
    public Transform GunPosition;
    
    #endregion

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _photonView = GetComponent<PhotonView>(); // Need to get this value to do networking
        _cController = GetComponent<CharacterController>();
        if (photonView.IsMine)
        {
            fpsCamera.enabled = true;
            localAudioListener.enabled = true;
            UiGameObject.SetActive(true);
            
        }
        UIGamePauseMenuGuns[4].SetActive(false);
        UIGamePauseMenuGuns[0].SetActive(true);
        
        _rb = GetComponent<Rigidbody>();
        Health = _maxHealth;
        
        if (UiGameObject && photonView.IsMine)
        {
            UiGameObject.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            playerUI = UiGameObject.GetComponent<PhotonPlayerUI>();
        }
        else
        {
            Debug.LogWarning("Missing uiPrefab reference on player prefab");
        }
        
        if (anim)
        {
            _animParams = anim.parameters;
        }
        
        _groundCheckPosition = transform.position - (new Vector3(0, _cController.height / 2, 0));

        // Gun Initilization 
        if (currentGun)
        {
            _currentGunInfo = currentGun.GetComponent<GunScript>();
            _currentGunInfo.SetOwner(this);
            _currentGunScriptable = _currentGunInfo.gunData;
            _fireRate = _currentGunInfo.GetFireRate();
        }
        
        var stack = fpsCamera.GetUniversalAdditionalCameraData();
        if(stack != null)
            stack.cameraStack.Add(_currentGunInfo.GetCamera().GetComponent<Camera>());
        
        /*
        #if DEBUG_VARIABLES
         // Checks to see if all required dependicies exists
         Debug.LogFormat("PhotonView exists: {0} on {1}", photonView, gameObject.name);
         Debug.LogFormat("CharacterController exists: {0} on {1}", (_cController), gameObject.name);
         Debug.LogFormat("CurrentGun exists: {0} on {1}", (currentGun), gameObject.name);
         Debug.LogFormat("Animator exists: {0} on {1}", (anim), gameObject.name);
         Debug.LogFormat("Camera exists: {0} on {1}", (fpsCamera), gameObject.name);
         Debug.LogFormat("LocalAudioListener exists: {0} on {1}", (localAudioListener), gameObject.name);
         Debug.LogFormat("CurrentGunScript exists: {0} on {1}", (_currentGunInfo), gameObject.name);
        #endif
        */
    }
    private void PauseGame()
    {
        isPaused = playerUI.TogglePause();
    }
    // Update is called once per frame
    private void Update()
    {
        if (!photonView.IsMine || _isDead)
            return;

        if(_currentGunInfo)
            _currentGunInfo.isAiming = Input.GetButton("Fire2");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        
        _cController.Move(_velocity * Time.deltaTime); // This calls to move the character downward based on gravity
        
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
                if(_currentGunInfo.Shoot())
                    photonView.RPC("ShootRPC", RpcTarget.All);
                
                _nextTimeToFire = Time.time + (1 / _fireRate);
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) // Change this to interactable axis later
        {
            Ray ray = fpsCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out _pickUpHit, pickupDistance, pickupMask))
            {
                GameObject newGun = _pickUpHit.transform.gameObject;
                Destroy(currentGun);
                SwitchGun(newGun);
                
            }
        }
        
        GroundCheck();
        
    }

    private void FixedUpdate()
    {
        // Update groundcheck location based on recent transform.position
        // Placed in FixedUpdate since we only want it to update every 60 ticks per second so its not based on fps
        _groundCheckPosition = transform.position - (new Vector3(0, _cController.height / 2, 0)); 
        _velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
    }

    private void SwitchGun(GameObject obj)
    {
        //obj.transform.parent = GunPosition.transform;
        obj.transform.localPosition = GunPosition.transform.position;
        obj.transform.parent = gunRotationPoint.transform;
        currentGun = obj;
        // Gun Initilization 
        if (currentGun)
        {
            _currentGunInfo = currentGun.GetComponent<GunScript>();
            _currentGunScriptable = _currentGunInfo.gunData;
            _currentGunInfo.SetOwner(this);
            _currentGunScriptable = _currentGunInfo.gunData;
            _fireRate = _currentGunInfo.GetFireRate();
            currentGun.layer = 12;
            var stack = fpsCamera.GetUniversalAdditionalCameraData();
            stack.cameraStack.RemoveAt(1);
            if (stack != null)
                stack.cameraStack.Add(_currentGunInfo.GetCamera().GetComponent<Camera>());
            if (_currentGunScriptable.gunIndex == 0)
            {
                currentGun.transform.localRotation = Quaternion.Euler(-90.0f, -90.0f, 0.0f);
                UIGamePauseMenuGuns[0].SetActive(true);
                UIGamePauseMenuGuns[1].SetActive(false);
                UIGamePauseMenuGuns[2].SetActive(false);
                UIGamePauseMenuGuns[3].SetActive(false);
                UIGamePauseMenuGuns[4].SetActive(false);
            } else if (_currentGunScriptable.gunIndex == 1)
            {
                currentGun.transform.localRotation = Quaternion.Euler(-90.0f, 90.0f, -180.0f);
                UIGamePauseMenuGuns[0].SetActive(false);
                UIGamePauseMenuGuns[1].SetActive(true);
                UIGamePauseMenuGuns[2].SetActive(false);
                UIGamePauseMenuGuns[3].SetActive(false);
                UIGamePauseMenuGuns[4].SetActive(false);
            } else if (_currentGunScriptable.gunIndex == 2)
            {
                currentGun.transform.localRotation = Quaternion.Euler(-90.0f, 90.0f, -180.0f);
                UIGamePauseMenuGuns[0].SetActive(false);
                UIGamePauseMenuGuns[1].SetActive(false);
                UIGamePauseMenuGuns[2].SetActive(true);
                UIGamePauseMenuGuns[3].SetActive(false);
                UIGamePauseMenuGuns[4].SetActive(false);
            }
            else if (_currentGunScriptable.gunIndex == 3)
            {
                currentGun.transform.localRotation = Quaternion.Euler(-90.0f, -90.0f, -180.0f);
                UIGamePauseMenuGuns[0].SetActive(false);
                UIGamePauseMenuGuns[1].SetActive(false);
                UIGamePauseMenuGuns[2].SetActive(false);
                UIGamePauseMenuGuns[3].SetActive(true);
                UIGamePauseMenuGuns[4].SetActive(false);
            }
        }
    }

    [PunRPC]
    private void ShootRPC()
    {
        _currentGunInfo.SharedActions();
    }

    private Color tempColor;
    public void SetPlayerColor(Color color)
    {
        tempColor= color;
        photonView.RPC("SetPlayerColorRPC", RpcTarget.All);
    }

    [PunRPC]
    private void SetPlayerColorRPC()
    {
        playerRenderer.material.color = tempColor;
        playerRenderer.UpdateGIMaterials();
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
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y + gravityConst));
            anim.SetTrigger(_animParams[1].name);
        }
    }

    private void Awake()
    {
        
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

    #region Photon Serialize View

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

    #endregion
    
    private void SetDeath()
    {
        anim.SetBool(_animParams[3].name, true);
        _isDead = true;
        Health = _maxHealth;
        currentGun.SetActive(false);
    }
    
    

    #region Public Methods

    public void SetPos(Transform pos)
    {
        teleportLocation = pos;
        photonView.RPC("RPCSetPos", RpcTarget.All);
    }
    
    public void ResetHealth()
    {
        Health = _maxHealth;
        _isDead = false;
    }

    #endregion
    
    #region Event Callbacks

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)EventManager.EventCodes.StartPreRound)
        {
            _isPreRound = true;
            currentGun.SetActive(true);
            if (anim)
            {
                anim.SetBool(_animParams[3].name, false);
            }

            _isDead = false;
            
        }else if (eventCode == (byte) EventManager.EventCodes.EndPreRound)
        {
            _isPreRound = false;
        }
    }

    #endregion

    #region Custom Callbacks

    // Want an OnGunPickup and an OnGunDrop
    // Both of these functions will call setting gun owner and appropriate info needed for each gun. 
    private void OnGunPickup(Action action)
    {
        action?.Invoke();
    }

    private void OnGunDrop(Action action)
    {
        action?.Invoke();
    }

    #endregion

    public void IGotShot(object damage)
    {
        photonView.RPC("IGotShotRPC", RpcTarget.All, (object)damage);
    }

    [PunRPC]
    private void IGotShotRPC(float damage)
    {
        Health -= (float)damage;
    }

    
    public void SetMyColor()
    {
        if (photonView.IsMine && photonView.name == PhotonLauncher.Instance.GetOtherPlayerName()) // This means you are the second player
        {
            playerRenderer.material.color = PhotonLauncher.Instance.GetPlayerTwoColor();
            Debug.LogFormat("Set my color called by {0} in condition 1", photonView);
        }else if (photonView.IsMine && PhotonNetwork.IsMasterClient)
        {
            playerRenderer.material.color = PhotonLauncher.Instance.GetPlayerOneColor();
            Debug.LogFormat("Set my color called by {0} in condition 2", photonView);
        }
        
        playerRenderer.UpdateGIMaterials();
        Debug.LogFormat("Set my color called by {0}, my color is {1}", photonView, playerRenderer.material.color);
        
        Debug.Log("-----------------------------");
        Debug.LogFormat("On pvID: {0}, player one stored color: {1}, player two stored color: {2}",
        photonView.ViewID,
        PhotonLauncher.Instance.GetPlayerOneColor(),
        PhotonLauncher.Instance.GetPlayerTwoColor()
        );
    }
    
    #region RPCCalls
    
    [PunRPC]
    private void RPCSetPos()
    {
        transform.position = teleportLocation.position;
        transform.rotation = teleportLocation.rotation;
        _rb.position = teleportLocation.position;
        _rb.rotation = teleportLocation.rotation;
        
    }

    #endregion

}
