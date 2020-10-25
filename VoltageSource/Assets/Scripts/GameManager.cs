using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

namespace VoltageSource
{
    public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static GameManager Instance;
        public GameObject playerPrefab;

        [SerializeField] private Transform yellowTeamSpawn;
        [SerializeField] private Transform blueTeamSpawn;

        [SerializeField] private int blueTeamKills = -1;
        [SerializeField] private int yellowTeamKills = -1;

        public GameObject[] blueTeamSide;
        public GameObject[] yellowTeamSide;

        private int playerOneID;
        private int playerTwoID;

        private void Start()
        {
            Instance = this;
            if (playerPrefab == null)
            {
                Debug.LogError("Missing player prefab on game manager");
                return;
            }

            if (yellowTeamSpawn == null || blueTeamSpawn == null)
            {
                Debug.LogError("One or more of the spawn points are missing");
                return;
            }
            Cursor.lockState = CursorLockMode.Locked;
            
            SpawnPlayers();
        }

   
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // This should be done by the gamemanager, not the player 
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Confined
                    ? CursorLockMode.None
                    : CursorLockMode.Confined;
            }
            
            if (Input.GetKey(KeyCode.Tab))
            {
                // Preform unconnect actions
                Cursor.lockState = CursorLockMode.Confined;
                LeaveRoom();
            }
        }

        public void RestartGame(int winnerID)
        {
            
        }

        public void BlueTeamIncrement()
        {
            blueTeamKills++;
            if (blueTeamKills >= 5)
            {
                EndGame();
                return;
            }
        }

        public void YellowTeamIncrement()
        {
            yellowTeamKills++;
            if (yellowTeamKills >= 5)
            {
                EndGame();
                return;
            }
        }

        public void EndGame()
        {
            Debug.Log("Game ended, congrats!");
        }


        private void SpawnPlayers()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(playerPrefab.name,
                    TeamManagerScript.Instance.PlayerOneTeam == 0 ? blueTeamSpawn.position : yellowTeamSpawn.position,
                    Quaternion.identity, 0);
            }
            else
            {
                PhotonNetwork.Instantiate(playerPrefab.name,
                    TeamManagerScript.Instance.PlayerTwoTeam == 0 ? blueTeamSpawn.position : yellowTeamSpawn.position,
                    Quaternion.identity, 0);
            }

        }
        
        #region Photon Callback

        public override void OnLeftRoom()
        {
            // This makes the assumption that the main menu is scene 0
            // which is typically the case
            SceneManager.LoadScene(0);
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public int GetPing()
        {
            return PhotonNetwork.GetPing();
        }

        public void SendProjectileRPC(GameObject prefab,Transform initialPos, Vector3 velocity, float lifeTime)
        {
            GameObject reference = PhotonNetwork.Instantiate(prefab.name, initialPos.position, initialPos.rotation, 0);
            reference.GetComponent<Rigidbody>().velocity = velocity;
            Destroy(reference, lifeTime);
        }
        
        #endregion

        #region Private Methods

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom);
            PhotonNetwork.LoadLevel(1);
        }

        #endregion

        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("OnPlayerLeftRoom {0}", otherPlayer.NickName);
        }
        
        

        #endregion

        public void OnEvent(EventData photonEvent)
        {
            byte eventcode = photonEvent.Code;

            if (eventcode == (byte)EventManager.EventCodes.PlayerDied)
            {
                object[] data = (object[]) photonEvent.CustomData;
                
                // Initiate pre-round event call
                // Initiate  
                
            }
        }

        private void EndRound()
        {
            
        }

        private void StartRound()
        {
            
        }
    }

}

