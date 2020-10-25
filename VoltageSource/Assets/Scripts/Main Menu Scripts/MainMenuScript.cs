using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using VoltageSource;


public class MainMenuScript : MonoBehaviourPun, IPunObservable, IOnEventCallback
{
    [SerializeField] private GameObject matchMakingBackground;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject matchMakingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject connecting;
    [SerializeField] private TMP_Dropdown playerOne;
    [SerializeField] private TMP_Dropdown playerTwo;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private Button startMatchButton;

    [SerializeField]
    private int _playerOneTeamChoice;
    [SerializeField]private int _playerTwoTeamChoice;

    // For creating the room
    private string _joinRoomName;
    private string _createRoomName;
    
    public Animator anim;
    private AnimatorControllerParameter[] _animParams; // Copied 

    #region Unity Defaults
    private void Start()
    {
        if (anim != null)
            _animParams = anim.parameters;
        InvokeRepeating("playWave", 10.0f, 10.0f);
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + PhotonLauncher.Instance.GetRoomCode().ToString();
        }

        _joinRoomName = null;
        _createRoomName = null;
        
    }
    void playWave()
    {
        anim.Play("Wave");
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion

    #region Button Calls

    public void MatchMakingButton()
        {
            background.SetActive(false);
            mainMenuPanel.SetActive(false);
            matchMakingBackground.SetActive(true);
            matchMakingPanel.SetActive(true);
            // To ensure is connected to master, if not then we won't be able to join rooms or stuff
            PhotonLauncher.Instance.Connect();
        }
    
        public void SettingsButton()
        {
            Debug.Log("Setting button on MainMenu pressed");
        }
    
        public void QuitGameButton()
        {
            Debug.Log("Quit Game button pressed");
            Application.Quit();
        }
    
        public void BackButton()
        {
            background.SetActive(true);
            mainMenuPanel.SetActive(true);
            matchMakingBackground.SetActive(false);
            matchMakingPanel.SetActive(false);
            connecting.SetActive(false);
        }

        public void CallLeaveRoom()
        {
            PhotonLauncher.Instance.LeaveRoom();
        }

        public void CallJoinRRoom()
        {
            connecting.SetActive(true);
            mainMenuPanel.SetActive(false);
            matchMakingPanel.SetActive(false);
            matchMakingBackground.SetActive(false);
            PhotonLauncher.Instance.JoinRRoom();
        }
        
        [PunRPC]
        public void StartMatch()
        {
            TeamManagerScript.Instance.PlayerOneTeam = _playerOneTeamChoice;
            TeamManagerScript.Instance.PlayerTwoTeam = _playerTwoTeamChoice;
        
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.MatchStart, null, raiseEventOptions, SendOptions.SendReliable);
            // Get info regarding who is on what team then start the game based on that and update teammanager
        }
        
        public void JoinRoom()
        {
            connecting.SetActive(true);
            background.SetActive(false);
            mainMenuPanel.SetActive(false);
            matchMakingPanel.SetActive(false);
            matchMakingBackground.SetActive(false);
            if (string.IsNullOrEmpty(_joinRoomName))
            {
                Debug.LogError("Room name is null or empty");
                return;
            }

            PhotonLauncher.Instance.JoinRoom(_joinRoomName);
        }
        
        public void CreateRoom()
        {
            connecting.SetActive(true);
            mainMenuPanel.SetActive(false);
            matchMakingPanel.SetActive(false);
            matchMakingBackground.SetActive(false);
            if (string.IsNullOrEmpty(_createRoomName))
            {
                Debug.LogError("Room name is null or empty");
                return;
            }
            PhotonLauncher.Instance.CreateRoom(_createRoomName);
        }
    #endregion

    public void SetJoinRoomName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Join Room name is null or empty");
            return;
        }

        _joinRoomName = value;
    }
    public void SetCreateRoomName(string value)
    {
        Debug.Log(value);
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Create Room name is null or empty");
            return;
        }
        
        _createRoomName = value;
    }

    
    private void TeamSelectP1(int value)
    {
        _playerOneTeamChoice = value;
        if (_playerOneTeamChoice == 0)
        {
            playerTwo.value = 1;
            _playerTwoTeamChoice = 1;
        }
        else
        {
            playerTwo.value = 0;
            _playerTwoTeamChoice = 0;
        }
        playerTwo.RefreshShownValue();
    }
    
    
    private void TeamSelectP2(int value)
    {
        _playerTwoTeamChoice = value;
        if (_playerTwoTeamChoice == 0)
        {
            playerOne.value = 1;
            _playerOneTeamChoice = 1;
            playerOne.RefreshShownValue();
        }
        else
        {
            playerOne.value = 0;
            _playerOneTeamChoice = 0;
            playerOne.RefreshShownValue();
        }
    }
    

    public void TeamSelectionChangeP1()
    {
        int playerOneValue = playerOne.value;
        string name = playerOne.gameObject.name;
        object[] content = new object[] {playerOneValue, name}; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.ChangeTeamSelection, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    public void TeamSelectionChangeP2()
    {
        int playerTwoValue = playerTwo.value;
        string name = playerTwo.gameObject.name;
        object[] content = new object[] {playerTwoValue, name}; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.ChangeTeamSelection, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == (byte)EventManager.EventCodes.ChangeTeamSelection)
        {
            object[] data = (object[])photonEvent.CustomData;
            // 0 index is value
            // 1 index is name of gameobject that changed
            if (playerOne.gameObject.name == (string)data[1])
            {
                TeamSelectP1((int)data[0]);
            }
            else
            {
                TeamSelectP2((int)data[0]);
            }
        }else if (eventCode == (byte)EventManager.EventCodes.MatchStart)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!PhotonLauncher.Instance.LaunchGameMatch())
                {
                    // do stuff that tells the person that there are not enough people in the match
                    // or have an button for the option to just go ahead and do single player. 
                }
            }
            
        }
    }

    #region Photon

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
        
    #endregion
    
}

