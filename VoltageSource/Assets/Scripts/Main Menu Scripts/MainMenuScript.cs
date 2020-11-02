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
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle privateGameToggle;

    #region CharacterColor Team Select
    /*
     * Yellow = 1;
     * Red = 2;
     * Green = 3;
     * Purple = 4;
     * Magenta = 5;
     * Black = 6;
     * Blue = 7;
     * Orange = 8;
     * Aqua = 9;
     * Maroon = 10;
     * White = 11;
    */
    // Materials
    [SerializeField] public Renderer Obj1, Obj2;
    [SerializeField] public Image YelB, RedB, GreB, PurB, MagB, BlkB, BluB, OrgB, AquB, MarB, WhtB;
    Image[] Images1 = new Image[11];
    Image[] Images2 = new Image[11];
    private Color yelCol, redCol, greCol, purCol, magCol, blkCol, bluCol, orgCol, aquCol, marCol, whtCol;
    Color[] Colors1 = new Color[11];
    Color[] Colors2 = new Color[11];
    [SerializeField] public int ActiveIndex1 = 0;
    [SerializeField] public int ActiveIndex2 = 0; // index of the current color and image selected in the arrays (+ 1)
    #endregion

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

        if (volumeSlider == null)
        {
            return;
        }
        volumeSlider.value = PlayerPrefs.HasKey("VolumeValue") ? PlayerPrefs.GetFloat("VolumeValue") : 1f;

        // Character Color Arrays
        yelCol = new Color(0.849746f, 0.9056604f, 0.39644f, 1);
        redCol = new Color(1, 0, 0, 1);
        greCol = new Color(0, 0.8679245f, 0.119f, 1);
        purCol = new Color(0.8362575f, 0, 1, 1);
        magCol = new Color(1, 0, 0.7459249f, 1);
        blkCol = new Color(0, 0, 0, 1);
        bluCol = new Color(0.0194375f, 0.3716098f, 0.8584906f, 1);
        orgCol = new Color(1, 0.4027772f, 0, 1);
        aquCol = new Color(0, 1, 0.7527807f, 1);
        marCol = new Color(0.5283019f, 0.0966f, 0.0966f, 1);
        whtCol = new Color(1, 1, 1, 1);
        Colors1[0] = yelCol;
        Colors1[1] = redCol;
        Colors1[2] = greCol;
        Colors1[3] = purCol;
        Colors1[4] = magCol;
        Colors1[5] = blkCol;
        Colors1[6] = bluCol;
        Colors1[7] = orgCol;
        Colors1[8] = aquCol;
        Colors1[9] = marCol;
        Colors1[10] = whtCol;
        Images1[0] = YelB;
        Images1[1] = RedB;
        Images1[2] = GreB;
        Images1[3] = PurB;
        Images1[4] = MagB;
        Images1[5] = BlkB;
        Images1[6] = BluB;
        Images1[7] = OrgB;
        Images1[8] = AquB;
        Images1[9] = MarB;
        Images1[10] = WhtB;
        Colors2[0] = yelCol;
        Colors2[1] = redCol;
        Colors2[2] = greCol;
        Colors2[3] = purCol;
        Colors2[4] = magCol;
        Colors2[5] = blkCol;
        Colors2[6] = bluCol;
        Colors2[7] = orgCol;
        Colors2[8] = aquCol;
        Colors2[9] = marCol;
        Colors2[10] = whtCol;
        Images2[0] = YelB;
        Images2[1] = RedB;
        Images2[2] = GreB;
        Images2[3] = PurB;
        Images2[4] = MagB;
        Images2[5] = BlkB;
        Images2[6] = BluB;
        Images2[7] = OrgB;
        Images2[8] = AquB;
        Images2[9] = MarB;
        Images2[10] = WhtB;
    }

    public void PrivateToggle()
    {
        PhotonLauncher.Instance.PrivateStatus(privateGameToggle);
    }
    
    void playWave()
    {
        if(!anim)
        {
            return;
        }
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
            CancelInvoke("playWave");
        }
    
        public void SettingsButton()
        {
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
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
            settingsPanel.SetActive(false);
            InvokeRepeating("playWave", 10f, 10f);
        }
        
        

        public void CallLeaveRoom()
        {
            PhotonLauncher.Instance.LeaveRoom();
        }

        public void CallJoinRRoom()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
                return;
            
            connecting.SetActive(true);
            mainMenuPanel.SetActive(false);
            matchMakingPanel.SetActive(false);
            matchMakingBackground.SetActive(false);
            PhotonLauncher.Instance.JoinRRoom();
        }
        
        [PunRPC]
        public void StartMatch()
        {
            if (PhotonNetwork.CountOfPlayers < 2)
            {
                Debug.LogError("Not enough players in the game");
                
            }
            
            TeamManagerScript.Instance.PlayerOneTeam = _playerOneTeamChoice;
            TeamManagerScript.Instance.PlayerTwoTeam = _playerTwoTeamChoice;
        
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.MatchStart, null, raiseEventOptions, SendOptions.SendReliable);
            // Get info regarding who is on what team then start the game based on that and update teammanager
            
        }
        
        public void JoinRoom()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
                return;
            
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
            if (!PhotonNetwork.IsConnectedAndReady)
                return;
            
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

    public void SetVolume()
    {
        AudioManager.Instance.SetVolume(volumeSlider.value);
    }
    
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

    public void ChracterColorP1(int color)
    {
        int holderIndex = ActiveIndex1;
        switch (color)
        {
            case 1:
                    Images1[0].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[0];
                    ActiveIndex1 = 1;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 2:
                    Images1[1].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[1];
                    ActiveIndex1 = 2;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 3:
                    Images1[2].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[2];
                    ActiveIndex1 = 3;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 4:
                    Images1[3].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[3];
                    ActiveIndex1 = 4;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 5:
                    Images1[4].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[4];
                    ActiveIndex1 = 5;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 6:
                    Images1[5].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[5];
                    ActiveIndex1 = 6;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 7:
                    Images1[6].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[6];
                    ActiveIndex1 = 7;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 8:
                    Images1[7].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[7];
                    ActiveIndex1 = 8;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 9:
                    Images1[8].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[8];
                    ActiveIndex1 = 9;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 10:
                    Images1[9].color = new Color32(100, 140, 160, 200);
                    Obj1.material.color = Colors1[9];
                    ActiveIndex1 = 10;
                    for (int i = 0; i < 11; i++)
                    {
                        if (i != (ActiveIndex1 - 1))
                            Images1[i].color = new Color32(255, 255, 255, 255);
                    }
                    break;
            case 11:
                Images1[10].color = new Color32(100, 140, 160, 200);
                Obj1.material.color = Colors1[10];
                ActiveIndex1 = 11;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex1 - 1))
                         Images1[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            default:
                Images1[11].color = new Color32(100, 140, 160, 200);
                Obj1.material.color = Colors1[11];
                ActiveIndex1 = 11;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex1 - 1))
                        Images1[i].color = new Color32(255, 255, 255, 255);
                }
            break;
        };
        if (holderIndex != ActiveIndex1)
        {
            object[] content = new object[] { holderIndex, ActiveIndex1 }; // Array contains the target position and the IDs of the selected units
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.ChangeCharacterColor1, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public void CharacterColorP2(int color)
    {
        int holderIndex = ActiveIndex2;
        switch (color)
        {
            case 1:
                Images2[0].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[0];
                ActiveIndex2 = 1;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 2:
                Images2[1].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[1];
                ActiveIndex2 = 2;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 3:
                Images2[2].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[2];
                ActiveIndex2 = 3;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 4:
                Images2[3].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[3];
                ActiveIndex2 = 4;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 5:
                Images2[4].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[4];
                ActiveIndex2 = 5;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 6:
                Images2[5].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[5];
                ActiveIndex2 = 6;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 7:
                Images2[6].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[6];
                ActiveIndex2 = 7;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 8:
                Images2[7].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[7];
                ActiveIndex2 = 8;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 9:
                Images2[8].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[8];
                ActiveIndex2 = 9;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 10:
                Images2[9].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[9];
                ActiveIndex2 = 10;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            case 11:
                Images2[10].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[10];
                ActiveIndex2 = 11;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
            default:
                Images2[11].color = new Color32(100, 140, 160, 200);
                Obj2.material.color = Colors2[11];
                ActiveIndex2 = 11;
                for (int i = 0; i < 11; i++)
                {
                    if (i != (ActiveIndex2 - 1))
                        Images2[i].color = new Color32(255, 255, 255, 255);
                }
                break;
        };
        if (holderIndex != ActiveIndex2)
        {
            object[] content = new object[] { holderIndex, ActiveIndex2 }; // Array contains the target position and the IDs of the selected units
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.ChangeCharacterColor2, content, raiseEventOptions, SendOptions.SendReliable);
        }
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
        } else if (eventCode == (byte)EventManager.EventCodes.ChangeCharacterColor1)
        {
            object[] data = (object[])photonEvent.CustomData;
            Obj1.UpdateGIMaterials();
            Images1[(int)data[1]].GraphicUpdateComplete();
        } else if (eventCode == (byte)EventManager.EventCodes.ChangeCharacterColor2)
        {
            object[] data = (object[])photonEvent.CustomData;
            Obj2.UpdateGIMaterials();
            Images2[(int)data[1]].GraphicUpdateComplete();
        }
    }

    #region Photon

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
        
    #endregion
    
}

