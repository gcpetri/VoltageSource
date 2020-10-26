using System.Collections;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;


namespace VoltageSource
{
    public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static GameManager Instance;
        public GameObject playerPrefab;

        [SerializeField] private Transform yellowTeamSpawn;
        [SerializeField] private Transform blueTeamSpawn;

        [SerializeField] private int blueTeamKills = 0;
        [SerializeField] private int yellowTeamKills = 0;

        public GameObject[] blueTeamSide;
        public GameObject[] yellowTeamSide;
        

        [SerializeField]private Material transparentMaterial;

        private RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        [SerializeField]private float preRoundTimer = 5f;
        [SerializeField] private float endRoundTimer = 10f;

        private GameObject _playerOne;
        private GameObject _playerTwo;
        
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
            // Call pre-round event, on this event players cannot move but can look around but can't shoot. 
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.StartPreRound, null, raiseEventOptions, SendOptions.SendReliable);
            
        }

  
        private void SpawnPlayers()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject PlayerOne = PhotonNetwork.Instantiate(playerPrefab.name,
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
        
        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public int GetPing()
        {
            return PhotonNetwork.GetPing();
        }

        /*
        public void SendProjectileRPC(GameObject prefab,Transform initialPos, Vector3 velocity, float lifeTime)
        {
            GameObject reference = PhotonNetwork.Instantiate(prefab.name, initialPos.position, initialPos.rotation, 0);
            reference.GetComponent<Rigidbody>().velocity = velocity;
            Destroy(reference, lifeTime);
        }
        */
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
        
        public override void OnLeftRoom()
        {
            // This makes the assumption that the main menu is scene 0
            // which is typically the case
            SceneManager.LoadScene(0);
        }

        #endregion

        #region PhotonEvents
        
        public void OnEvent(EventData photonEvent)
        {
            byte eventcode = photonEvent.Code;
            object[] data = null;
            if (photonEvent.CustomData != null)
            {
                data = photonEvent.CustomData as object[];
            }

            switch (eventcode)
            {
                case (byte)EventManager.EventCodes.PlayerDied: PlayerDied(data);
                    break;
                case (byte)EventManager.EventCodes.StartRound: StartRound();
                    break;
                case (byte)EventManager.EventCodes.EndRound: EndRound();
                    break;
                case (byte)EventManager.EventCodes.StartPreRound: StartPreRound();
                    break;
                case (byte)EventManager.EventCodes.EndPreRound: EndPreRound();
                    break;
                default: break;
            }
            
        }

        private void PlayerDied(object[] data = null)
        {
            if (data == null)
            {
                Debug.LogError("Data needed for PlayerDied is missing");
                return;
            }
        
            // 0 photonViewID of the player that died
            if (PhotonView.Find((int) data[0]).IsMine)
            {
                // This means the host died instead
                if (TeamManagerScript.Instance.PlayerOneTeam == 0) // Which means blue team
                    BlueTeamIncrement();
                else
                    YellowTeamIncrement();
            }
            else
            {
                if (TeamManagerScript.Instance.PlayerTwoTeam == 0) // Which means blue team
                    BlueTeamIncrement();
                else
                    YellowTeamIncrement();
            }
            
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.EndRound, null, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void EndRound(object[] data = null)
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            
            Debug.Log("Round ended");

            StartCoroutine(IEndRound());
            // Based on # of kills for each player, apply the appropriate actions to the level
            // Change level's pickups to either enabled or disabled and set appropriate materials to level cover and properties 

        }

        private IEnumerator IEndRound()
        {
            yield return new WaitForSeconds(endRoundTimer);
            // Spawn other stuff
            for (int i = 0; i < yellowTeamKills; i++)
            {
                if (i >= 2)
                    break;
                
                foreach (MeshRenderer obj in blueTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
            }
            for (int i = 0; i < blueTeamKills; i++)
            {
                if (i >= 2)
                    break;
                
                foreach (MeshRenderer obj in yellowTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
            }

            _playerOne.transform.position = TeamManagerScript.Instance.PlayerOneTeam == 0
                ? blueTeamSpawn.position
                : yellowTeamSpawn.position;
            
            _playerTwo.transform.position = TeamManagerScript.Instance.PlayerTwoTeam == 0
                ? blueTeamSpawn.position
                : yellowTeamSpawn.position;
            
            _playerOne.transform.rotation = TeamManagerScript.Instance.PlayerOneTeam == 0
                ? blueTeamSpawn.rotation
                : yellowTeamSpawn.rotation;
            
            _playerTwo.transform.rotation = TeamManagerScript.Instance.PlayerTwoTeam == 0
                ? blueTeamSpawn.rotation
                : yellowTeamSpawn.rotation;

            _playerOne.GetComponent<FpController>().ResetHealth();
            _playerTwo.GetComponent<FpController>().ResetHealth();
            
            Debug.Log("End of EndRound event coroutine");
            PhotonNetwork.RaiseEvent((byte) EventManager.EventCodes.StartPreRound, null, raiseEventOptions,
                SendOptions.SendReliable);

        }
        

        private void StartRound(object[] data = null)
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            
            Debug.Log("Round started");
        }
        
        private void RestartGame(int winnerID)
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
        }

        private void BlueTeamIncrement()
        {
            blueTeamKills++;
            Debug.Log(blueTeamKills);
            if (blueTeamKills >= 5)
            {
                EndGame();
                return;
            }
        }

        private void YellowTeamIncrement()
        {
            yellowTeamKills++;
            Debug.Log(yellowTeamKills);
            if (yellowTeamKills >= 5)
            {
                EndGame();
                return;
            }
        }

        private void EndGame()
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            Debug.Log("EndGame() Called");
        }

        private void StartPreRound()
        {
            StartCoroutine(PreRound());
        }

        private IEnumerator PreRound()
        {
            Debug.Log("Pre round started");
            yield return new WaitForSeconds(preRoundTimer);
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.EndPreRound, null, raiseEventOptions, SendOptions.SendReliable);
            
        }

        private void EndPreRound()
        {
            Debug.Log("Pre round Ended");
            // If the pre-round ends then start the actual round 
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.StartRound, null, raiseEventOptions, SendOptions.SendReliable);
        }
        
        #endregion
    }

}

