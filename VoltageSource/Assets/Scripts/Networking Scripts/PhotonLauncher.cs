using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace VoltageSource
{
    public class PhotonLauncher : MonoBehaviourPunCallbacks
    {
        private string gameVersion = "1";

        #region Private Serializable Fields

        [Tooltip("The max num of players per room")]
        [SerializeField]
        private byte maxPlayersPerRoom = 2;

        [SerializeField] private GameObject controlPanel;
        [SerializeField] private GameObject progressLabel;

        #endregion
        
        private bool _isConnecting;
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        public void Connect()
        {
            Debug.LogFormat("Trying to Join room with nickname: {0}", PhotonNetwork.NickName.ToString());
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                
                _isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #region MonoBehaviourPunCallBacks CallBacks

        public override void OnConnectedToMaster()
        {
            if (_isConnecting)
            {
                Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
                PhotonNetwork.JoinRandomRoom();
                _isConnecting = false;
            }
            
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause.ToString());
           
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions{MaxPlayers =  maxPlayersPerRoom});
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the level 1");
                PhotonNetwork.LoadLevel(1);
            }
            //Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            //PhotonNetwork.LoadLevel(1);
        }

        
        #endregion
        
    }
}
