using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

namespace VoltageSource
{
    public class GameManager : MonoBehaviourPunCallbacks
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
            
            foreach (Player obj in PhotonNetwork.PlayerList)
            {
                Debug.Log(PhotonNetwork.PlayerList.Length);
                // Then assume player is Player one
                if (obj.IsMasterClient)
                {
                    Debug.Log("Master client called");
                    if (TeamManagerScript.Instance.PlayerOneTeam == 0) // This is if player is on the blue team 
                    {
                        PhotonNetwork.Instantiate(playerPrefab.name, blueTeamSpawn.position, Quaternion.identity, 0);
                    }
                    else
                    {
                        PhotonNetwork.Instantiate(playerPrefab.name, yellowTeamSpawn.position, Quaternion.identity, 0);
                    }
                        
                }
                else
                {
                    if (TeamManagerScript.Instance.PlayerTwoTeam == 0)
                    {
                        PhotonNetwork.Instantiate(playerPrefab.name, blueTeamSpawn.position, Quaternion.identity, 0);
                    }
                    else
                    {
                        PhotonNetwork.Instantiate(playerPrefab.name, yellowTeamSpawn.position, Quaternion.identity, 0);
                    }
                        
                }
            }
                
            Cursor.lockState = CursorLockMode.Locked; // Should be handled by a seperate script not player script
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

        public void SetPlayerOneID(int id)
        {
            playerOneID = id;
        }

        public void SetPlayerTwoID(int id)
        {
            playerTwoID = id;
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
            GameObject reference =
                PhotonNetwork.Instantiate(prefab.name, initialPos.position, initialPos.rotation, 0);
            reference.GetComponent<Rigidbody>().velocity = velocity;
            Destroy(reference, lifeTime);
        }
        
        #endregion

        #region Private Methods

        void LoadAreana()
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
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadAreana();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("OnPlayerLeftRoom {0}", otherPlayer.NickName);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                
                LoadAreana();
            }
        }

        #endregion
        
    }

}

