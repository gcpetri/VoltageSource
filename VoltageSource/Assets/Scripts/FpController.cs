#define DEBUG_VARIABLES
#define DEBUG_METHODS

using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
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
    private bool _isPreRound = true;


    #endregion

    #region Character Movement Variables
    // Contains all the variables used from FP_movement 
    [Header("Character Movement Variables")]
    [SerializeField] private CharacterController _cController; // Reference
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

                object[] content = { photonView.ViewID };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.PlayerDied, content, raiseEventOptions, SendOptions.SendReliable);
                photonView.RPC("SetDeath", RpcTarget.AllBuffered);
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
    [SerializeField] private GameObject[] FPguns;
    #endregion

    #region Animator Variables

    //[Header("Animator variables")] [SerializeField]
    [SerializeField] private Animator anim; // Reference 

    [SerializeField] private AnimatorControllerParameter[] _animParams; // Copied 

    #endregion

    #region UI Variables

    [SerializeField] private GameObject UiGameObject; // Reference 
    private PhotonPlayerUI playerUI;
    private bool isPaused = false;
    [SerializeField] public GameObject[] UIGamePauseMenuGuns;
    // End of Game UI
    public GameObject EndofGameCuties;
    public Renderer playerRender1;
    public Renderer playerRender2;
    [SerializeField] public Animator[] EndofGameCutiesAnimators;
    [SerializeField] public Slider AmmoSlider;

    #endregion

    [SerializeField] private Renderer playerRenderer;

    private Rigidbody _rb;
    [SerializeField] private AudioListener localAudioListener; // Reference
    private Transform teleportLocation;
    [SerializeField] private GameObject _uimission;

    #region GunPickup Variables

    [Header("Gun Pickup Variables")] [SerializeField]
    private LayerMask pickupMask;
    [SerializeField] private float pickupDistance = 5f;
    private RaycastHit _pickUpHit;
    public Transform GunPosition;

    #endregion

    private Color[] colorChoices = new Color[11];

    private bool _gameEnded = false;

    
    private void Start()
    {
        _cController = GetComponent<CharacterController>();
        for (int i = 0; i < 11; i++)
        {
            colorChoices[i] = CharacterColorChoices.ColorChoices[i];
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        _photonView = GetComponent<PhotonView>(); // Need to get this value to do networking
        
        if (photonView.IsMine)
        {
            fpsCamera.enabled = true;
            localAudioListener.enabled = true;
            UiGameObject.SetActive(true);
        }
        SetMyColor();
        
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
            for (int i = 0; i < 3; i++)
                anim.SetBool(_animParams[i].name, false);
        }

        _groundCheckPosition = transform.position - (new Vector3(0, _cController.height / 2, 0));

        // Gun Initilization 
        if (currentGun)
        {
            _currentGunInfo = currentGun.GetComponent<GunScript>();
            _currentGunInfo.SetOwner(this);
            _currentGunScriptable = _currentGunInfo.gunData;
            _fireRate = _currentGunInfo.GetFireRate();
            if (!this.photonView.IsMine)
                SetLayerRecursively(currentGun, 9);
        }

        var stack = fpsCamera.GetUniversalAdditionalCameraData();
        if (stack != null)
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
        if (!photonView.IsMine)
        {
            if (_rb.velocity != Vector3.zero)
            {
                anim.SetBool(_animParams[0].name, true); // running for other player
            }else
                anim.SetBool(_animParams[0].name, false); // idle for other player
            
            return;
        }

        if (_isDead)
            return;
        
        if (_currentGunInfo)
            _currentGunInfo.isAiming = Input.GetButton("Fire2");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gameEnded)
                return;
            PauseGame();
        }

        _cController.Move(_velocity * Time.deltaTime); // This calls to move the character downward based on gravity

        if (isPaused || _gameEnded)
            return;

        InputProcess();


        if (_isPreRound)
            return;

        if (Input.GetButton("Fire1"))
        {
            // The if statement is used just to reduce number of RPC calls between clients
            if (_nextTimeToFire < Time.time)
            {
                if (_currentGunInfo.Shoot())
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
                currentGun.SetActive(false);
                SwitchGun2(newGun);
                Destroy(newGun);
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

    #region GunPickUps Methods
    // method that switches already present guns in the FPcharacter
    private void SwitchGun2(GameObject obj)
    {
        SetGun(obj.GetComponent<GunScript>().gunData.gunIndex);
        int data = obj.GetComponent<GunScript>().gunData.gunIndex;
        photonView.RPC("SetPlayerGunRPC", RpcTarget.AllBuffered, (int) data);
    }
    public void SetGun(int i)
    {
        FPguns[i].SetActive(true);
        FPguns[i].layer = 12;
        SetLayerRecursively(FPguns[i], 12);
        UIGamePauseMenuGuns[i].SetActive(true);
        _currentGunInfo = FPguns[i].GetComponent<GunScript>();
        _currentGunScriptable = _currentGunInfo.gunData;
        _currentGunInfo._currentAmmo = _currentGunScriptable.maxAmmo;
        _currentGunInfo.SetOwner(this);
        _currentGunScriptable = _currentGunInfo.gunData;
        currentGun = FPguns[i];
        currentGun.SetActive(true);
        _fireRate = _currentGunInfo.GetFireRate();
        AmmoSlider.maxValue = _currentGunScriptable.maxAmmo;
        AmmoSlider.value = _currentGunScriptable.maxAmmo;
        var stack = fpsCamera.GetUniversalAdditionalCameraData();
        stack.cameraStack.RemoveAt(1);
        if (stack != null)
            stack.cameraStack.Add(_currentGunInfo.GetCamera().GetComponent<Camera>());
        for (int j = 0; j < 4; j++)
        {
            if (j != i)
            {
                FPguns[j].SetActive(false);
                FPguns[j].layer = 11;
                SetLayerRecursively(FPguns[j], 11);
                UIGamePauseMenuGuns[j].SetActive(false);
            }
        }
    }
    public static void SetLayerRecursively(GameObject obj, int layerNumber)
    {
        foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }
    #endregion

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
    // Added this to try to change the other player copy of gun
    [PunRPC]
    private void SetPlayerGunRPC(int data, PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == this.photonView.ViewID && this.photonView.IsMine) // Player one made the call so update that copy's gun
        {
            for (int i = 0; i < 4; i++)
            {
                FPguns[i].SetActive(false);
            }
            FPguns[data].SetActive(true);
            currentGun = FPguns[data];
        } else if (info.photonView.ViewID == this.photonView.ViewID && !this.photonView.IsMine) // Player one made the call so update that copy's gun
        {
            for (int i = 0; i < 4; i++)
            {
                FPguns[i].SetActive(false);
            }
            FPguns[data].SetActive(true);
            SetLayerRecursively(FPguns[data], 9); // changed so hopefully other player's guns don't move with your gun camera
            currentGun = FPguns[data];
        }
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
        anim.SetBool(_animParams[1].name, false);
        anim.SetBool(_animParams[2].name, false);
        if (xAxis != 0 || zAxis != 0)
            anim.SetBool(_animParams[0].name, true); // I'm running
        else
            anim.SetBool(_animParams[0].name, false); // I'm idle

        if (Input.GetButtonDown("Jump") && _isGrounded) // Checks if jumps and runs appropriate code
        {
            anim.SetBool(_animParams[0].name, false);
            anim.SetBool(_animParams[1].name, false);
            anim.SetBool(_animParams[2].name, false);
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y + gravityConst));
            anim.SetTrigger(_animParams[3].name);
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

    #region Photon Serialize View

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!photonView.IsMine)
            return;
        
        
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
    
    [PunRPC]
    private void SetDeath(PhotonMessageInfo info)
    { 
        AmmoSlider.maxValue = _currentGunScriptable.maxAmmo;
        AmmoSlider.value = _currentGunScriptable.maxAmmo;
        if (photonView.ViewID != info.photonView.ViewID)
        {
            return;
        }
        anim.SetBool(_animParams[2].name, true); // I'm dead
        _isDead = true;
        Health = _maxHealth;
        SetGun(0);
        currentGun.SetActive(false);
    }
    
    

    #region Public Methods

    public void SetPos(Transform pos)
    {
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
            SetPos(null);
            ResetHealth();
            _currentGunInfo._currentAmmo = _currentGunInfo.gunData.maxAmmo;
            _isPreRound = true;
            currentGun.SetActive(true);
            if (anim)
            {
                anim.SetBool(_animParams[0].name, false);
                anim.SetBool(_animParams[1].name, false);
                anim.SetBool(_animParams[2].name, false);
            }
            AmmoSlider.maxValue = _currentGunScriptable.maxAmmo;
            AmmoSlider.value = _currentGunScriptable.maxAmmo;
            _isDead = false;
            
        }else if (eventCode == (byte) EventManager.EventCodes.EndPreRound)
        {
            _uimission.SetActive(false);
            _isPreRound = false;
        }
    }

    #endregion
    

    public void IGotShot(object damage)
    {
        photonView.RPC("IGotShotRPC", RpcTarget.All, (object)damage);
    }

    public void SetMyColor()
    {
        photonView.RPC("RPCColorChange", RpcTarget.AllBuffered);
    }
    public void EndtheGame(int[] data)
    {
        photonView.RPC("RPCEndGameColorChange", RpcTarget.All, (int[])data);
    }
    
    
    
    #region RPCCalls
    
    [PunRPC]
    private void RPCSetPos()
    {
        /*
        transform.position = teleportLocation.position;
        transform.rotation = teleportLocation.rotation;
        _rb.position = teleportLocation.position;
        _rb.rotation = teleportLocation.rotation;
        */
        
        if (photonView.IsMine && photonView.ViewID == 1001) // If This is player 1 and is player 1's character
        {
            transform.position = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            transform.rotation = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
            _rb.position = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            _rb.rotation = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
        }
        else if(photonView.IsMine && photonView.ViewID == 2001) // If This is player 2 and is player 2's character
        {
            transform.position = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            transform.rotation = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
            _rb.position = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            _rb.rotation = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
        }


        if (!photonView.IsMine && photonView.ViewID == 1001) // If This is player 1 and is player 2's character
        {
            transform.position = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            transform.rotation = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
            _rb.position = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            _rb.rotation = TeamManagerScript.Instance.PlayerOneTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
            
        }else if (!photonView.IsMine && photonView.ViewID == 2001) // If This is player 2 and is player 1's character
        {
            transform.position = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            transform.rotation = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;
            _rb.position = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.position : GameManager.Instance.yellowTeamSpawn.position;
            _rb.rotation = TeamManagerScript.Instance.PlayerTwoTeam == 0 ? GameManager.Instance.blueTeamSpawn.rotation : GameManager.Instance.yellowTeamSpawn.rotation;;
        }
    }
    
    
    [PunRPC]
    public void RPCColorChange()
    {

        Debug.LogFormat("On pvID: {0}, player one stored color: {1}, player two stored color: {2}",
            photonView.ViewID,
            PhotonLauncher.Instance.GetPlayerOneColor(),
            PhotonLauncher.Instance.GetPlayerTwoColor()
        );

        
        if (photonView.IsMine && photonView.ViewID == 1001)
        {
            playerRenderer.material.color =
                CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerOneColor()];
            
            Debug.LogFormat("Playerone color called on id: {0} color: {1} ", photonView.ViewID, PhotonLauncher.Instance.GetPlayerOneColor());
        }
        else if(photonView.IsMine && photonView.ViewID == 2001)
        {
            playerRenderer.material.color =
                CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
            
            Debug.LogFormat("Playertwo color called on id: {0} color: {1} ", photonView.ViewID, PhotonLauncher.Instance.GetPlayerTwoColor());
        }


        if (!photonView.IsMine && photonView.ViewID == 1001)
        {
            playerRenderer.material.color =
                CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerOneColor()];
        }else if (!photonView.IsMine && photonView.ViewID == 2001)
        {
            playerRenderer.material.color =
                CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
        }
        
        playerRenderer.UpdateGIMaterials();
    }
    
    [PunRPC]
    private void IGotShotRPC(float damage)
    {
        Health -= (float)damage;
    }

    [PunRPC]
    private void RPCEndGameColorChange(int[] data)
    {
        _gameEnded = true;
        if (!photonView.IsMine)
            return;
        EndofGameCuties.SetActive(true);
        EndofGameCuties.GetComponent<Camera>().enabled = true;
        fpsCamera.enabled = false;
        playerRender1.material.color = CharacterColorChoices.ColorChoices[data[0]];
        playerRender2.material.color = CharacterColorChoices.ColorChoices[data[1]];
        playerRender1.UpdateGIMaterials();
        playerRender2.UpdateGIMaterials();
        // set their dancing and dying animation
        for (int i = 0; i < 2; i++) 
            for (int j = 0; j < 3; j++)
                EndofGameCutiesAnimators[i].SetBool(EndofGameCutiesAnimators[i].parameters[j].name, false);

        EndofGameCutiesAnimators[0].SetBool(EndofGameCutiesAnimators[0].parameters[1].name, true);
        EndofGameCutiesAnimators[1].SetBool(EndofGameCutiesAnimators[1].parameters[2].name, true);
        EndofGameCuties.SetActive(true);
    }

    #endregion

}
