using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using VoltageSource;


public class MainMenuScript : MonoBehaviourPun, IPunObservable
{

    [SerializeField] private GameObject matchMakingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TMP_Dropdown playerOne;
    [SerializeField] private TMP_Dropdown playerTwo;
    [SerializeField] private TMP_Text roomCodeText;


    private int _playerOneTeamChoice;
    private int _playerTwoTeamChoice;

    

    private void Start()
    {
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + PhotonLauncher.Instance.GetRoomCode().ToString();
        }
    }

    public void MatchMakingButton()
    {
        mainMenuPanel.SetActive(false);
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
        mainMenuPanel.SetActive(true);
        matchMakingPanel.SetActive(false);
    }

    [PunRPC]
    public void TeamSelectP1()
    {
        _playerOneTeamChoice = playerOne.value;
        if (_playerOneTeamChoice == 0)
        {
            playerTwo.value = 1;
            _playerTwoTeamChoice = 1;
            playerTwo.RefreshShownValue();
        }
        else
        {
            playerTwo.value = 0;
            _playerTwoTeamChoice = 0;
            playerTwo.RefreshShownValue();
        }
    }

    [PunRPC]
    public void TeamSelectP2()
    {
        _playerTwoTeamChoice = playerTwo.value;
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

    public void CallLeaveRoom()
    {
        PhotonLauncher.Instance.LeaveRoom();
        
    }

    public void CallJoinRRoom()
    {
        PhotonLauncher.Instance.JoinRRoom();
    }

    public void StartMatch()
    {
        // Get info regarding who is on what team then start the game based on that and update teammanager
        if (!PhotonLauncher.Instance.LoadGameScene())
        {
            // do stuff that tells the person that there are not enough people in the match
            // or have an button for the option to just go ahead and do single player. 
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_playerOneTeamChoice);
            stream.SendNext(_playerTwoTeamChoice);
        }else
        {
            _playerOneTeamChoice = (int)stream.ReceiveNext();
            _playerTwoTeamChoice = (int)stream.ReceiveNext();
        }
        
        
    }
}

