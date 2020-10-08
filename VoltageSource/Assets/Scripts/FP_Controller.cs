using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using VoltageSource;

// Require component tells unity when you add this script to a game object
// that it must also add these components as well as they are required for the
// script to run
[RequireComponent(typeof(PhotonView), 
    typeof(Rigidbody), 
    typeof(CharacterController))]
public class FP_Controller : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Instances

    public static GameObject LocalPlayerInstance;

    #endregion

    #region Networking Variables

    private PhotonView _photonView;
    [SerializeField] private AudioListener localAudioListener;

    #endregion
    
    #region Character Movement Variables
    // Contains all the variables used from FP_movement 
    [Header("Character Movement Variables")]
    private CharacterController _cController;
    [Tooltip("Controls the speed at which the player moves")]
    [SerializeField] private float playerSpeed = 12f;
    [Tooltip("The height at which the player can jump")]
    [SerializeField] private float jumpHeight = 3f;
    [Tooltip("Empty gameobject at the bottom of the character model")]
    [SerializeField] private GameObject groundCheck;
    [Tooltip("Layer mask for what is considered ground to check if player is grounded")]
    [SerializeField] private LayerMask groundMask;
    [Tooltip("Radius of sphere cast at groundCheck location to check if grounded")]
    [SerializeField] private float groundDistance = 0.3f;

    private Vector3 _velocity;
    private bool _isGrounded;
    #endregion

    #region Player Health Varaibles

    // Handle being hit and store health. Best done by get and set funcitons
        [SerializeField] private float health;
        public float Health
        {
            get => health;
            set
            {
                Debug.LogErrorFormat("PhotonView {0} took damage, current health is {1}", photonView.ViewID, health);
                if (health <= 0)
                {
                    Debug.Log("Died");
                    // If player dies then all function to handle their death
                }
                health = value;
            }
        }

    #endregion

    #region Camera Variables

    [Header("Camera Variables")] [SerializeField]
    private float lookSensitivity = 100f;

    private float _xRotation = 0f;
    [SerializeField] private Camera fpsCamera;

    #endregion

    #region Gun Variables

    [Header("Gun Variables")] [SerializeField]
    private GameObject currentGun;

    private GunScript _currentGunInfo;
    #endregion

    #region Animator Variables

    [Header("Animator variables")] [SerializeField]
    private Animator anim;

    private AnimatorControllerParameter[] _animParams;

    #endregion

    #region UI Variables

    [SerializeField] private GameObject uiPrefab;
    

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>(); // Need to get this value to do networking
        Cursor.lockState = CursorLockMode.Locked;
        _cController = GetComponent<CharacterController>();
        _currentGunInfo = currentGun.GetComponent<GunScript>();
        if (photonView.IsMine)
        {
            fpsCamera.enabled = true;
            localAudioListener.enabled = true;
        }

        if (uiPrefab != null)
        {
            GameObject _uiGo = Instantiate(uiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("Missing uiPrefab reference on player prefab");
        }

        if (anim != null)
        {
            _animParams = anim.parameters;
        }
    }
    
    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = this.gameObject;
        }

        DontDestroyOnLoad(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            // Preform unconnect actions
            Cursor.lockState = CursorLockMode.Confined;
            GameManager.Instance.LeaveRoom();
        }


        if (Input.GetButton("Fire2"))
            _currentGunInfo.isAiming = true;
        else
            _currentGunInfo.isAiming = false;

        if (Input.GetButton("Fire1"))
            photonView.RPC("ShootRPC", RpcTarget.All, null);
        
        GroundCheck();
        InputProcess();


        _velocity.y += Physics.gravity.y * Time.deltaTime;
        _cController.Move(_velocity * Time.deltaTime);

        
    }

    /// <summary>
    /// Processes all the inputs required for the FP_Controller
    /// </summary>
    private void InputProcess()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float zAxis = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X") * (lookSensitivity * Time.deltaTime);
        float mouseY = Input.GetAxis("Mouse Y") * (lookSensitivity * Time.deltaTime);

        MovePlayer(xAxis, zAxis); // Moves the player
        MoveCamera(mouseX, mouseY); // Moves the camera

        if (xAxis != 0 || zAxis != 0)
        {
            anim.SetBool(_animParams[0].name, true);
        }
        else
        {
            anim.SetBool(_animParams[0].name, false);
        }
        
        if (Input.GetButtonDown("Jump") && _isGrounded) // Checks if jumps and runs appropriate code
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            anim.SetTrigger(_animParams[3].name);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Confined
                ? CursorLockMode.None
                : CursorLockMode.Confined;
        }
    }

    #region Movement/Camera Methods

    /// <summary>
        /// Performs the check to see if the FP_Controller is grounded
        /// </summary>
        private void GroundCheck()
        {
            _isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }
    
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
        }

    #endregion

    [PunRPC]
    private void ShootRPC()
    {
        _currentGunInfo.Shoot(photonView.ViewID);
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
