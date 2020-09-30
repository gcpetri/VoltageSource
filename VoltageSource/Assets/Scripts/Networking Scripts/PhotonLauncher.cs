using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

namespace VoltageSource
{
    public class PhotonLauncher : MonoBehaviourPunCallbacks
    {
        public string gameVersion = "1";
        public static PhotonLauncher Instance;
        

        #region Private Serializable Fields

        [Tooltip("The max num of players per room")]
        [SerializeField]
        private byte maxPlayersPerRoom = 2;

        [SerializeField] private GameObject controlPanel;
        [SerializeField] private GameObject progressLabel;
        [SerializeField] private Toggle _privateToggle;

        #endregion
        
        private bool _isConnecting;
        private bool _isPrivate = true; // changes based on the toggle
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            Instance = this;
            Connect();
        }

        public void Connect()
        {
            Debug.LogFormat("Trying to Join room with nickname: {0}", PhotonNetwork.NickName.ToString());
            if (!PhotonNetwork.IsConnected)
            {
                _isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public void CreateRoom(string value)
        {
            
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.CreateRoom(value, new RoomOptions {MaxPlayers = maxPlayersPerRoom, IsVisible = !_isPrivate});
            else
                Connect();
        }

        /// <summary>
        /// Called JoinRRoom for JoinRandomRoom to avoid naming conflicts
        /// </summary>
        public void JoinRRoom()
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.JoinRandomRoom();
            else
                Connect();
        }

        public void JoinRoom(string value)
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.JoinRoom(value);
            else
                Connect();
            
        }

        public void PrivateStatus()
        {
            _isPrivate = _privateToggle.isOn;
            Debug.Log(_isPrivate);
        }
        
        #region MonoBehaviourPunCallBacks CallBacks

        public override void OnConnectedToMaster()
        {
            if (_isConnecting)
            {
                Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
                _isConnecting = false;
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause.ToString());
           
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions{MaxPlayers =  maxPlayersPerRoom});
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("We load the level 1");
            PhotonNetwork.LoadLevel(1);
            //if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            //{
                //Debug.Log("We load the level 1");
                //PhotonNetwork.LoadLevel(1);
            //}
            //Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            //PhotonNetwork.LoadLevel(1);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError("Failed to create room");
        }


        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogErrorFormat("Failed to join room: Error Code: {0}", returnCode.ToString());
        }

        #endregion
        
    }
}
