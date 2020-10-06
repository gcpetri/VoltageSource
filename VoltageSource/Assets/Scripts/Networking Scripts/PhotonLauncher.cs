using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [SerializeField] private int MainMenuIndex;
        [SerializeField] private int TeamSelectIndex;
        [SerializeField] private int GameSceneIndex;
        
        #endregion
        
        private bool _isConnecting;
        private bool _isPrivate = true; // changes based on the toggle
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }else if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            
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

        public string GetHostName()
        {
            return PhotonNetwork.MasterClient.NickName;
        }

        public string GetOtherPlayerName()
        {
            string otherPlayer = "Player Two";
            foreach (Player obj in PhotonNetwork.PlayerList)
            {
                if(!obj.IsMasterClient)
                {
                    otherPlayer = obj.NickName;
                }
            }

            return otherPlayer;
        }

        public string GetRoomCode()
        {
            return PhotonNetwork.CurrentRoom.Name;
        }
        
        /// <summary>
        /// This is only to be used for leaving the team select scene
        /// </summary>
        public void LeaveRoom()
        {
            if (!PhotonNetwork.IsConnected)
                return;
            Debug.Log("LeaveRoom() was called");
            PhotonNetwork.LeaveRoom();
        }

        public void LoadTeamSelect()
        {
            PhotonNetwork.LoadLevel(TeamSelectIndex);
        }

        public void LoadGameScene()
        {
            PhotonNetwork.LoadLevel(GameSceneIndex);
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
            // Generate random room name which is a 5 digit code
            System.Random generator = new System.Random();
            String nameCode = generator.Next(0, 99999).ToString("D5");
            while(PhotonNetwork.CreateRoom(nameCode, new RoomOptions{MaxPlayers =  maxPlayersPerRoom}))
            {
                nameCode = generator.Next(0, 99999).ToString("D5");
            }
            
        }

        public override void OnJoinedRoom()
        {
            Debug.LogFormat("OnJoinedRoom called loading level {0}", TeamSelectIndex);
            PhotonNetwork.LoadLevel(TeamSelectIndex);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError("Failed to create room");
        }


        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogErrorFormat("Failed to join room: Error Code: {0}", returnCode.ToString());
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(MainMenuIndex);
            base.OnLeftRoom();
        }

        #endregion
        
    }
}
