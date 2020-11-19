using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering.Universal;
using WebSocketSharp;


namespace VoltageSource
{
    public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static GameManager Instance;
        public GameObject playerPrefab;
        [CanBeNull] public Transform yellowTeamSpawn;
        [CanBeNull] public Transform blueTeamSpawn;

        [SerializeField] private int blueTeamDeaths = 0;
        [SerializeField] private int yellowTeamDeaths = 0;

        public GameObject[] blueTeamSide;
        public GameObject[] yellowTeamSide;
        public GameObject[] gunPrefabs; // 0 = pistol, 1 = assault rifle, 2 = shotgun, 3 = sniper

        // Gun Spawning
        public GameObject gunWallPrefab;
        public Transform[] blueGunSpawns;
        public Transform[] yellowGunSpawns;

        private int[]
            numGunSpawnsPerSeg =
                {3, 5, 9, 14, 20}; // Increments: seg 5 (closest to player) has 6, seg 1 (farthest) has 3

        private int _blueGunSpawnRange = 20;
        private int _yellowGunSpawnRange = 20;

        private GameObject
            _yellowWall, _blueWall, _yellowGun, _blueGun; 

        public int BlueSegments = 5;

        public int YellowSegments = 5;

        //public float BlueTerrScale = 75.0f;
        public float minGunSpawnTime = 25.0f;
        public float maxGunSpawnTime = 35.0f;

        // End of Round UI
        public GameObject EndofRoundEmpty;
        [SerializeField] public GameObject[] UIEndofRound = new GameObject[10];

        [SerializeField] private Material transparentMaterial;

        // End of Game UI
        public GameObject EndofGameUI;
        public Text GameWinner;
        public GameObject One;

        private RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        public float preRoundTimer = 5f;
        public float endRoundTimer = 10f;

        private GameObject _playerOne;
        private GameObject _playerTwo;

        bool boolGameOver = false; // the game is over\

        [SerializeField] private Material floorTex;


        private void Start()
        {
            AudioManager.Instance.StopAudio();
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
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;

            PhotonNetwork.RaiseEvent((byte) EventManager.EventCodes.StartPreRound, null, raiseEventOptions,
                SendOptions.SendReliable);
            boolGameOver = false;
        }


        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void SpawnPlayers()
        {
            if ((blueTeamSpawn is null) || (yellowTeamSpawn is null))
            {
                Debug.LogError("SpawnPlayers() called, missing blueTeamSpawn and/or yellowTeamSpawn");
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                _playerOne = PhotonNetwork.Instantiate(playerPrefab.name,
                    TeamManagerScript.Instance.PlayerOneTeam == 0 ? blueTeamSpawn.position : yellowTeamSpawn.position,
                    TeamManagerScript.Instance.PlayerOneTeam == 0 ? blueTeamSpawn.rotation : yellowTeamSpawn.rotation);

            }
            else
            {
                _playerTwo = PhotonNetwork.Instantiate(playerPrefab.name,
                    TeamManagerScript.Instance.PlayerTwoTeam == 0 ? blueTeamSpawn.position : yellowTeamSpawn.position,
                    TeamManagerScript.Instance.PlayerTwoTeam == 0 ? blueTeamSpawn.rotation : yellowTeamSpawn.rotation);

            }
            
            for (int i = 0; i < 5; i++)
            {
                if (PhotonLauncher.Instance.GetPlayerOneColor() == PhotonLauncher.Instance.GetPlayerTwoColor())
                    return;
                
                if (TeamManagerScript.Instance.PlayerOneTeam == 0)
                {
                    blueTeamSide[i].GetComponent<MeshRenderer>().material.color =
                        CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerOneColor()];
                    UIEndofRound[i].GetComponent<Image>().color =
                                CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                }
                else
                {
                    yellowTeamSide[i].GetComponent<MeshRenderer>().material.color =
                        CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerOneColor()];
                    UIEndofRound[i].GetComponent<Image>().color =
                            CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerOneColor()];
                }
                
                if (TeamManagerScript.Instance.PlayerTwoTeam == 0)
                {
                    blueTeamSide[i].GetComponent<MeshRenderer>().material.color =
                        CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                    UIEndofRound[i+5].GetComponent<Image>().color =
                            CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                }
                else
                {
                    yellowTeamSide[i].GetComponent<MeshRenderer>().material.color =
                        CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                    UIEndofRound[i+5].GetComponent<Image>().color =
                            CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerOneColor()];
                } 
            }
        }

        #region Game End

        // End the Game
        private void EndtheGame()
        {
            boolGameOver = true;
            Debug.Log("EndtheGame() called");
            Cursor.lockState = CursorLockMode.Confined;
            int[] data = {0, 0};
            if (blueTeamDeaths >= 5) // Yellow Won
            {
                if (TeamManagerScript.Instance.PlayerOneTeam == 0) // player one is blue
                {
                    data[0] = PhotonLauncher.Instance.GetPlayerTwoColor();
                    data[1] = PhotonLauncher.Instance.GetPlayerOneColor();
                }
                else // player one is yellow
                {
                    data[0] = PhotonLauncher.Instance.GetPlayerOneColor();
                    data[1] = PhotonLauncher.Instance.GetPlayerTwoColor();
                }
            }
            else if (yellowTeamDeaths >= 5) // Blue Won
            {
                if (TeamManagerScript.Instance.PlayerOneTeam == 0) // player one is blue
                {
                    data[0] = PhotonLauncher.Instance.GetPlayerOneColor();
                    data[1] = PhotonLauncher.Instance.GetPlayerTwoColor();
                }
                else // player one is yellow
                {
                    data[0] = PhotonLauncher.Instance.GetPlayerTwoColor();
                    data[1] = PhotonLauncher.Instance.GetPlayerOneColor();
                }
            }

            if (data[0] == PhotonLauncher.Instance.GetPlayerOneColor())
                GameWinner.text = PhotonLauncher.Instance.GetHostName(); // player one name
            else
                GameWinner.text = PhotonLauncher.Instance.GetOtherPlayerName(); // player two name
            
            
            EndofGameUI.SetActive(true);
            if(_playerOne)
                _playerOne.GetComponent<FpController>().EndtheGame(data);
            
            if(_playerTwo)
                _playerTwo.GetComponent<FpController>().EndtheGame(data);
            

            
            for (int i = 0; i < Mathf.Clamp(blueTeamDeaths, 0, 5); i++)
            {
                foreach (MeshRenderer obj in blueTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
                blueTeamSide[i].GetComponent<MeshRenderer>().material = floorTex;
            }
            for (int i = 0; i < Mathf.Clamp(yellowTeamDeaths, 0, 5); i++)
            {
                foreach (MeshRenderer obj in yellowTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
                yellowTeamSide[i].GetComponent<MeshRenderer>().material = floorTex;
            }
            
            
            
            StopCoroutine(SpawnGunAfterTime());

            //One.GetComponent<Animator>().SetBool(One.GetComponent<Animator>().parameters[0].name, true)
        }
        
        
        
        #endregion

        #region Gun Spawn
        
        private void SendGunSpawnEvent()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int gunIndexB =
                    UnityEngine.Random.Range(0, gunPrefabs.Length); // Get random index between what guns exist
                //var blueRangeHorizontal = UnityEngine.Random.Range(-75.0f, BlueTerrScale);
                //Vector3 blueSpawnPos = new Vector3(blueRangeHorizontal, 1.0f, UnityEngine.Random.Range(-70.0f, 70.0f));
                int blueSpawnPos;

                switch (blueTeamDeaths)
                {
                    case 1: blueSpawnPos = UnityEngine.Random.Range(0, _blueGunSpawnRange - 3);
                        break;
                    case 2: blueSpawnPos = UnityEngine.Random.Range(0, _blueGunSpawnRange - 5);
                        break;
                    case 3: blueSpawnPos = UnityEngine.Random.Range(0, _blueGunSpawnRange - 9);
                        break;
                    case 4: blueSpawnPos = UnityEngine.Random.Range(0, _blueGunSpawnRange - 14);
                        break;
                    default: blueSpawnPos = UnityEngine.Random.Range(0, _blueGunSpawnRange);
                        break;
                }
                

                int gunIndexY = UnityEngine.Random.Range(0, 3);
                //float yellowRangeHorizontal = UnityEngine.Random.Range(BlueTerrScale, 75.0f);
                //Vector3 YellowSpawnPos = new Vector3(yellowRangeHorizontal, 1.0f, UnityEngine.Random.Range(-70.0f, 70.0f));
                int yellowSpawnPos;
                
                switch (yellowTeamDeaths)
                {
                    case 1: yellowSpawnPos = UnityEngine.Random.Range(0, _yellowGunSpawnRange - 3);
                        break;
                    case 2: yellowSpawnPos = UnityEngine.Random.Range(0, _yellowGunSpawnRange - 5);
                        break;
                    case 3: yellowSpawnPos = UnityEngine.Random.Range(0, _yellowGunSpawnRange - 9);
                        break;
                    case 4: yellowSpawnPos = UnityEngine.Random.Range(0, _yellowGunSpawnRange - 14);
                        break;
                    default: yellowSpawnPos = UnityEngine.Random.Range(0, _yellowGunSpawnRange);
                        break;
                }
                

                var content = new object[] {gunIndexB, blueSpawnPos, gunIndexY, yellowSpawnPos};

                PhotonNetwork.RaiseEvent((byte) EventManager.EventCodes.SpawnGun, content, raiseEventOptions,
                    SendOptions.SendReliable);

                //Debug.Log("Gun Spawn Called");
            }
            
            
        }

        // spawns the gun prefabs over time
        IEnumerator SpawnGunAfterTime()
        {
            SendGunSpawnEvent();
            
            float spawnTime = 30f;
            yield return new WaitForSeconds(spawnTime - 4.0f);
            
            if(_yellowWall)
                _yellowWall.GetComponent<Flashing>().StartFlashing();
            
            if(_blueWall)
                _blueWall.GetComponent<Flashing>().StartFlashing();
     
            yield return new WaitForSeconds(4f);
            
            StartCoroutine(SpawnGunAfterTime());
        }
        

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            AudioManager.Instance.StartAudio();
            Cursor.lockState = CursorLockMode.Confined;
            PhotonNetwork.LeaveRoom();
            Destroy(this);
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
            if (boolGameOver)
                return;
            
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
                case (byte)EventManager.EventCodes.EndRound: EndRound(data);
                    break;
                case (byte)EventManager.EventCodes.StartPreRound: StartPreRound();
                    break;
                case (byte)EventManager.EventCodes.EndPreRound: EndPreRound();
                    break;
                case (byte)EventManager.EventCodes.SpawnGun: SpawnGun(data);
                    break;
                default: break;
            }
            
        }

        private void SpawnGun(object[] data)
        {
            if (data == null)
                return;
            
            if (_yellowWall)
            {
                Destroy(_yellowWall);
                Destroy(_blueWall);
            }
            if (_yellowGun)
                Destroy(_yellowGun);
            if (_blueGun)
                Destroy(_blueGun);
            
            _blueWall = Instantiate(gunWallPrefab, blueGunSpawns[(int)data[1]]) as GameObject;
            _yellowWall = Instantiate(gunWallPrefab, yellowGunSpawns[(int)data[3]]) as GameObject;
            
            
            _blueGun = Instantiate(gunPrefabs[(int)data[0]], _blueWall.transform) as GameObject;
            
            
            _blueGun.transform.localPosition += 2.3f * Vector3.up + -1f * Vector3.forward;
            if ((int)data[0] == 0) // rotate the pistol
                _blueGun.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
            else if ((int)data[0] == 3) // rotate the sniper
                _blueGun.transform.Rotate(new Vector3(90.0f, 90.0f, 90.0f));
            
            _yellowGun = Instantiate(gunPrefabs[(int)data[2]], _yellowWall.transform) as GameObject;
            _yellowGun.transform.localPosition += 2.3f * Vector3.up + -1f* Vector3.forward;
            
            if ((int)data[2] == 0) // rotate the pistol
                _yellowGun.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
            else if ((int)data[2] == 3) // rotate the sniper
                _yellowGun.transform.Rotate(new Vector3(90.0f, 90.0f, 90.0f));
            
        }

        private void PlayerDied(object[] data = null)
        {
            if (data == null)
            {
                Debug.LogError("Data needed for PlayerDied is missing");
                return;
            }
            int Btemp = blueTeamDeaths;
            int Ytemp = yellowTeamDeaths;
            // 0 photonViewID of the player that died
            if ((int)data[0] == 1001)
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
            
            
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            //if (boolGameOver) return;
            object[] content = { Btemp, blueTeamDeaths, Ytemp, yellowTeamDeaths };
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.EndRound, content, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void EndRound(object[] data)
        {
            Debug.Log("Round ended");
            if (!boolGameOver)
            {
                EndofRoundEmpty.SetActive(true); // Ui map
            }
            StartCoroutine(IEndRound());
            StopCoroutine(SpawnGunAfterTime());
            
            if (_yellowWall)
            {
                Destroy(_yellowWall);
            }
            if(_blueWall)
                Destroy(_blueWall);
            
            if (_yellowGun)
                Destroy(_yellowGun);
            if (_blueGun)
                Destroy(_blueGun);
            
            
            if (boolGameOver)
                return;
            
            for (int i = 0; i < 10; i++)
                UIEndofRound[i].SetActive(false);
            
            //Animator a;
            
            // blue UI segments
            if ((int)data[1] < 5)
            {
                for (int i = (int)data[1] + 5; i < 10; i++)
                {
                    UIEndofRound[i].SetActive(true);
                }
            }
            // yellow UI segments
            if ((int)data[3] < 5)
            {
                for (int i = (int)data[3]; i < 5; i++)
                {
                    UIEndofRound[i].SetActive(true);
                }
            }
            
            /*if ((int)data[0] != (int)data[1] && (int)data[1] < 5) // blue lost one
            {
                UIEndofRound[(int)data[1] + 5].SetActive(true);
            }
            else if ((int)data[2] != (int)data[3] && (int)data[3] < 5) // yellow lost one
            {
                UIEndofRound[(int)data[3] + 1].SetActive(true);
            }*/
            
            // Based on # of segments for each player, apply the appropriate actions to the level
            // Change level's pickups to either enabled or disabled and set appropriate materials to level cover and properties 

        }
        private IEnumerator IEndRound()
        {
            if (_yellowWall)
            {
                Destroy(_yellowWall);
            }
            if(_blueWall)
                Destroy(_blueWall);
            
            if (_yellowGun)
                Destroy(_yellowGun);
            if (_blueGun)
                Destroy(_blueGun);

            if (!boolGameOver)
            {
                yield return new WaitForSeconds(endRoundTimer);
            }

            // Spawn other stuff
            for (int i = 0; i < Mathf.Clamp(blueTeamDeaths, 0, 5); i++)
            {
                foreach (MeshRenderer obj in blueTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
                
                blueTeamSide[i].GetComponent<MeshRenderer>().material = floorTex;
            }
            for (int i = 0; i < Mathf.Clamp(yellowTeamDeaths, 0, 5); i++)
            {
                foreach (MeshRenderer obj in yellowTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
                
                yellowTeamSide[i].GetComponent<MeshRenderer>().material = floorTex;
            }

            if (boolGameOver)
                yield return null;
            
            if (_playerOne)
            {
                _playerOne.GetComponent<FpController>().SetPos(TeamManagerScript.Instance.PlayerOneTeam == 0
                                ? blueTeamSpawn
                                : yellowTeamSpawn);
            }
            

            if (_playerTwo)
            {
                _playerTwo.GetComponent<FpController>().SetPos(TeamManagerScript.Instance.PlayerTwoTeam == 0
                                ? blueTeamSpawn
                                : yellowTeamSpawn);
            }
            
            
            Debug.Log("End of EndRound event coroutine");
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                yield return null;
            
            
            PhotonNetwork.RaiseEvent((byte) EventManager.EventCodes.StartPreRound, null, raiseEventOptions,
                SendOptions.SendReliable);
                
        }
        

        private void StartRound(object[] data = null)
        {
            StartCoroutine(SpawnGunAfterTime());
            EndofRoundEmpty.SetActive(false);
        }
        
        private void RestartGame(int winnerID)
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
        }

        private void BlueTeamIncrement()
        {
            blueTeamDeaths++;
            BlueSegments--;
            Debug.Log(BlueSegments);
            if (blueTeamDeaths >= 5)
            {
                EndtheGame();
                boolGameOver = true;
            }
        }

        private void YellowTeamIncrement()
        {
            yellowTeamDeaths++;
            YellowSegments--;
            Debug.Log(YellowSegments);
            if (yellowTeamDeaths >= 5)
            {
                EndtheGame();
                boolGameOver = true;
            }
        }

        public bool IsGameOver()
        {
            return boolGameOver;
        }

        private void StartPreRound()
        {
            EndofRoundEmpty.SetActive(false);
            StartCoroutine(PreRound());
        }

        private IEnumerator PreRound()
        {
            Debug.Log("Pre round started");
            yield return new WaitForSeconds(preRoundTimer);
            
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                yield return null;
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.EndPreRound, null, raiseEventOptions, SendOptions.SendReliable);
            
        }

        private void EndPreRound()
        {
            Debug.Log("Pre round Ended");
            // If the pre-round ends then start the actual round 
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.StartRound, null, raiseEventOptions, SendOptions.SendReliable);
        }
        
        #endregion
    }

}