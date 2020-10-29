using System;
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
        public GameObject[] gunPrefabs;

        // Gun Spawning
        public int BlueSegments = 5;
        public int YellowSegments = 5;
        public float BlueTerrScale = 75.0f;
        public float minGunSpawnTime = 15.0f;
        public float maxGunSpawnTime = 45.0f;
        //

        [SerializeField]private Material transparentMaterial;

        private RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        public float preRoundTimer = 5f;
        public float endRoundTimer = 10f;

        private GameObject _playerOne;
        private GameObject _playerTwo;
        
        private void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);
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

            SpawnPlayers();
            // Call pre-round event, on this event players cannot move but can look around but can't shoot. 
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.StartPreRound, null, raiseEventOptions, SendOptions.SendReliable);
        }


        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void SpawnPlayers()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                 _playerOne = PhotonNetwork.Instantiate(playerPrefab.name,
                    TeamManagerScript.Instance.PlayerOneTeam == 0 ? blueTeamSpawn.position : yellowTeamSpawn.position,
                    TeamManagerScript.Instance.PlayerOneTeam == 0 ? blueTeamSpawn.rotation : yellowTeamSpawn.rotation, 0);
            }
            else
            {
                 _playerTwo =PhotonNetwork.Instantiate(playerPrefab.name,
                    TeamManagerScript.Instance.PlayerTwoTeam == 0 ? blueTeamSpawn.position : yellowTeamSpawn.position,
                    TeamManagerScript.Instance.PlayerOneTeam == 0 ? blueTeamSpawn.rotation : yellowTeamSpawn.rotation, 0);
            }
        }

        #region Gun Spawn
        // spawns the guns at random time with inumerator
        public void SpawnRandomLocation()
        {
            StartCoroutine(SpawnGunAfterTime());
        }
        // gun spawn function
        public void GunSpawn()
        {
            int gunIndexB = UnityEngine.Random.Range(0, 3);
            float blueRangeHorizontal = UnityEngine.Random.Range(-75.0f, BlueTerrScale);
            Vector3 BlueSpawnPos = new Vector3(blueRangeHorizontal, 2.0f, UnityEngine.Random.Range(-70.0f, 70.0f));
            GameObject blueGun = Instantiate(gunPrefabs[gunIndexB], BlueSpawnPos, Quaternion.identity) as GameObject;
            int gunIndexY = UnityEngine.Random.Range(0, 3);
            float yellowRangeHorizontal = UnityEngine.Random.Range(BlueTerrScale, 75.0f);
            Vector3 YellowSpawnPos = new Vector3(yellowRangeHorizontal, 2.0f, UnityEngine.Random.Range(-70.0f, 70.0f));
            GameObject yellowGun = Instantiate(gunPrefabs[gunIndexY], YellowSpawnPos, Quaternion.identity) as GameObject;
            Debug.Log("spawned gun");
        }
        // spawns the gun prefabs over time
        IEnumerator SpawnGunAfterTime()
        {
            float spawnTime = UnityEngine.Random.Range(minGunSpawnTime, maxGunSpawnTime);
            yield return new WaitForSeconds(spawnTime);
            GunSpawn();
        }
        // determines the territory range for spawning 
        private void SpawnLocations(int BlueSegments)
        {
            switch(BlueSegments)
            {
                case 10:
                    BlueTerrScale = 75.0f;
                    break;
                case 9:
                    BlueTerrScale = 60.0f;
                    break;
                case 8:
                    BlueTerrScale = 45.0f;
                    break;
                case 7:
                    BlueTerrScale = 30.0f;
                    break;
                case 6:
                    BlueTerrScale = 15.0f;
                    break;
                // even   Blue Losing below   ///  Blue Winning above
                case 5:
                    BlueTerrScale = 0.0f;
                    break;
                case 4:
                    BlueTerrScale = -15.0f;
                    break;
                case 3:
                    BlueTerrScale = -30.0f;
                    break;
                case 2:
                    BlueTerrScale = -45.0f;
                    break;
                case 1:
                    BlueTerrScale = -60.0f;
                    break;
                case 0:
                    BlueTerrScale = -75.0f;
                    break;
                default:
                    BlueTerrScale = 0.0f;
                    break;
            }
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
            StopCoroutine(SpawnGunAfterTime());
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
            
            FpController fpsReference;
            
            if (_playerOne != null)
            {
                fpsReference = _playerOne.GetComponent<FpController>();
                fpsReference.SetPos(TeamManagerScript.Instance.PlayerTwoTeam == 0
                    ? blueTeamSpawn
                    : yellowTeamSpawn);
                
                fpsReference.ResetHealth();      
            }

            if (_playerTwo != null)
            {
                fpsReference = _playerTwo.GetComponent<FpController>();
                fpsReference.SetPos(TeamManagerScript.Instance.PlayerTwoTeam == 0
                    ? blueTeamSpawn
                    : yellowTeamSpawn);
                
                fpsReference.ResetHealth();
            }
            
            Debug.Log("End of EndRound event coroutine");
            PhotonNetwork.RaiseEvent((byte) EventManager.EventCodes.StartPreRound, null, raiseEventOptions,
                SendOptions.SendReliable);
        }
        

        private void StartRound(object[] data = null)
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            SpawnLocations(BlueSegments);
            GunSpawn();
            SpawnRandomLocation();
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
            BlueSegments++;
            YellowSegments--;
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
            BlueSegments--;
            YellowSegments++;
        }

        private void EndGame()
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            Debug.Log("EndGame() Called");
            StopCoroutine(SpawnGunAfterTime());
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

