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


namespace VoltageSource
{
    public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static GameManager Instance;
        public GameObject playerPrefab;

        [SerializeField] [CanBeNull] private Transform yellowTeamSpawn;
        [SerializeField] [CanBeNull] private Transform blueTeamSpawn;

        [SerializeField] private int blueTeamDeaths = 0;
        [SerializeField] private int yellowTeamDeaths = 0;

        public GameObject[] blueTeamSide;
        public GameObject[] yellowTeamSide;
        public GameObject[] gunPrefabs;

        // Gun Spawning
        public GameObject gunWallPrefab;
        public Transform[] blueGunSpawns;
        public Transform[] yellowGunSpawns;
        private int[] numGunSpawnsPerSeg = { 3, 5, 9, 14, 20 }; // Increments: seg 5 (closest to player) has 6, seg 1 (farthest) has 3
        private int _blueGunSpawnRange = 20;
        private int _yellowGunSpawnRange = 20;
        private GameObject _yW, _bW, _yG, _bG; // Not very descriptive, not sure what they could be used for from the outset 

        public int BlueSegments = 5;
        public int YellowSegments = 5;
        //public float BlueTerrScale = 75.0f;
        public float minGunSpawnTime = 25.0f;
        public float maxGunSpawnTime = 35.0f;

        // End of Round UI
        public GameObject EndofRoundEmpty;
        [SerializeField] public GameObject[] UIEndofRound;
        public Renderer playerRender1;
        public Renderer playerRender2;
        // End of Game UI
        public GameObject EndofGameEmpty;
        public GameObject EndofGameCuties;
        public GameObject[] EndofGameCutieModels;
        public Text GameWinner;
        [SerializeField] public Animator[] EndofGameCutiesAnimators;

        [SerializeField]private Material transparentMaterial;

        private RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        public float preRoundTimer = 5f;
        public float endRoundTimer = 10f;

        private GameObject _playerOne;
        private GameObject _playerTwo;
        
        private IEnumerator _gunSpawnEnumerator;

        private void Start()
        {
            _gunSpawnEnumerator = SpawnGunAfterTime();
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
            
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.StartPreRound, null, raiseEventOptions, SendOptions.SendReliable);
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
        }

        #region Gun Spawn
        // spawns the guns at random time with Enumerator 
        private void SpawnRandomLocation()
        {
            StartCoroutine(_gunSpawnEnumerator);
        }
        // gun spawn function
        private void GunSpawn()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int gunIndexB = UnityEngine.Random.Range(0, gunPrefabs.Length); // Get random index between what guns exist
                //var blueRangeHorizontal = UnityEngine.Random.Range(-75.0f, BlueTerrScale);
                //Vector3 blueSpawnPos = new Vector3(blueRangeHorizontal, 1.0f, UnityEngine.Random.Range(-70.0f, 70.0f));
                int blueSpawnPos = UnityEngine.Random.Range(0, _blueGunSpawnRange);

                int gunIndexY = UnityEngine.Random.Range(0, 3);
                //float yellowRangeHorizontal = UnityEngine.Random.Range(BlueTerrScale, 75.0f);
                //Vector3 YellowSpawnPos = new Vector3(yellowRangeHorizontal, 1.0f, UnityEngine.Random.Range(-70.0f, 70.0f));
                int yellowSpawnPos = UnityEngine.Random.Range(0, _yellowGunSpawnRange);

                var content = new object[] {gunIndexB, blueSpawnPos, gunIndexY, yellowSpawnPos};

                PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.SpawnGun, content, raiseEventOptions, SendOptions.SendReliable);
                
                Debug.Log("Gun Spawn Called");
            }
        }
        
        // spawns the gun prefabs over time
        IEnumerator SpawnGunAfterTime()
        {
            float spawnTime = UnityEngine.Random.Range(minGunSpawnTime, maxGunSpawnTime);
            yield return new WaitForSeconds(spawnTime);
            GunSpawn();
        }
        
        // determines the territory range for gun spawning 
        private void SpawnLocations(int blueSegs)
        {
            switch(blueSegs)
            {
                case 10:
                    //BlueTerrScale = 75.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = 0;
                    break;
                case 9:
                    //BlueTerrScale = 60.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[0];
                    break;
                case 8:
                    //BlueTerrScale = 45.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[1];
                    break;
                case 7:
                    //BlueTerrScale = 30.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[2];
                    break;
                case 6:
                    //BlueTerrScale = 15.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[3];
                    break;
                // even   Blue Losing below   ///  Blue Winning above
                case 5:
                    //BlueTerrScale = 0.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
                    break;
                case 4:
                    //BlueTerrScale = -15.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[3];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
                    break;
                case 3:
                    //BlueTerrScale = -30.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[2];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
                    break;
                case 2:
                    //BlueTerrScale = -45.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[1];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
                    break;
                case 1:
                    //BlueTerrScale = -60.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[0];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
                    break;
                case 0:
                    //BlueTerrScale = -75.0f;
                    _blueGunSpawnRange = 0;
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
                    break;
                default:
                    //BlueTerrScale = 0.0f;
                    _blueGunSpawnRange = numGunSpawnsPerSeg[4];
                    _yellowGunSpawnRange = numGunSpawnsPerSeg[4];
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

        private void SpawnGun(object[] data = null)
        {
            if (data == null)
                return;
            if (_yW)
            {
                Destroy(_yW);
                Destroy(_bW);
            }
            if (_yG)
                Destroy(_yG);
            if (_bG)
                Destroy(_bG);
            //Instantiate(gunPrefabs[(int) data[0]], (Vector3)data[1] + (0.5f * Vector3.up),Quaternion.identity);
            //Instantiate(gunPrefabs[(int) data[2]], (Vector3)data[3] + (0.5f * Vector3.up),Quaternion.identity);
            _bW = Instantiate(gunWallPrefab, blueGunSpawns[(int)data[1]]) as GameObject;
            _yW = Instantiate(gunWallPrefab, yellowGunSpawns[(int)data[3]]) as GameObject;
            _bG = Instantiate(gunPrefabs[(int)data[0]], _bW.transform) as GameObject;
            _bG.transform.localPosition += 2.3f * Vector3.up + -1f * Vector3.forward;
            _bG.transform.localRotation = Quaternion.identity;
            _yG = Instantiate(gunPrefabs[(int)data[2]], _yW.transform) as GameObject;
            _yG.transform.localPosition += 2.3f * Vector3.up + -1f* Vector3.forward;
            _yG.transform.localRotation = Quaternion.identity;
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
            if (PhotonView.Find((int) data[0]).gameObject == _playerOne)
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
            object[] content = { Btemp, blueTeamDeaths, Ytemp, yellowTeamDeaths };
            PhotonNetwork.RaiseEvent((byte)EventManager.EventCodes.EndRound, content, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void EndRound(object[] data)
        {
            Debug.Log("Round ended");
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            EndofRoundEmpty.SetActive(true); // Ui map
            StartCoroutine(IEndRound());
            for (int i = 0; i < 10; i++)
                UIEndofRound[i].SetActive(false);
            // blue UI segments
            if ((int)data[0] != (int)data[1] && (int)data[1] < 5)
            {
                for (int i = (int)data[1] + 5; i < 10; i++)
                {
                    UIEndofRound[(int)data[i]].SetActive(true);
                }
                UIEndofRound[(int)data[0] + 5].SetActive(true);
                UIEndofRound[(int)data[0] + 5].GetComponent<Animation>().Play();
                StartCoroutine(IEndRoundUI(UIEndofRound[(int)data[0] + 5]));
                StopCoroutine(IEndRoundUI(UIEndofRound[(int)data[0] + 5]));
            }
            // yellow UI segments
            if ((int)data[2] != (int)data[3] && (int)data[3] < 5)
            {
                for (int i = (int)data[3]; i < 5; i++)
                {
                    UIEndofRound[(int)data[i]].SetActive(true);
                }
                UIEndofRound[(int)data[2]].SetActive(true);
                UIEndofRound[(int)data[2]].GetComponent<Animation>().Play();
                StartCoroutine(IEndRoundUI(UIEndofRound[(int)data[2]]));
                StopCoroutine(IEndRoundUI(UIEndofRound[(int)data[2]]));
            }
            StopCoroutine(SpawnGunAfterTime());
            // Based on # of segments for each player, apply the appropriate actions to the level
            // Change level's pickups to either enabled or disabled and set appropriate materials to level cover and properties 

        }
        private IEnumerator IEndRoundUI(GameObject ui)
        {
            yield return new WaitForSeconds(3.2f);
            ui.SetActive(false);
        }
        private IEnumerator IEndRound()
        {
            yield return new WaitForSeconds(endRoundTimer);
            // Spawn other stuff
            for (int i = 0; i < yellowTeamDeaths; i++)
            {
                foreach (MeshRenderer obj in blueTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
            }
            for (int i = 0; i < blueTeamDeaths; i++)
            {
                foreach (MeshRenderer obj in yellowTeamSide[i].GetComponentsInChildren<MeshRenderer>())
                {
                    obj.material = transparentMaterial;
                }
            }
            
            FpController fpsReference;
            
            if (_playerOne)
            {
                if (_playerOne.GetComponent<PhotonView>().IsMine)
                {
                    fpsReference = _playerOne.GetComponent<FpController>();
                                    fpsReference.SetPos(TeamManagerScript.Instance.PlayerTwoTeam == 0
                                        ? blueTeamSpawn
                                        : yellowTeamSpawn);
                }
            }

            if (_playerTwo)
            {
                if (_playerTwo.GetComponent<PhotonView>().IsMine)
                {
                    fpsReference = _playerTwo.GetComponent<FpController>();
                    fpsReference.SetPos(TeamManagerScript.Instance.PlayerTwoTeam == 0
                        ? blueTeamSpawn
                        : yellowTeamSpawn);
                    
                }
            }
            
            Debug.Log("End of EndRound event coroutine");
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                yield return null;
            
            
            PhotonNetwork.RaiseEvent((byte) EventManager.EventCodes.StartPreRound, null, raiseEventOptions,
                SendOptions.SendReliable);
        }
        

        private void StartRound(object[] data = null)
        {
            if (!PhotonNetwork.IsMasterClient) // So it doesn't run on other clients 
                return;
            EndofRoundEmpty.SetActive(false);
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
            blueTeamDeaths++;
            BlueSegments--;
            Debug.Log(BlueSegments);
            if (blueTeamDeaths <= 5)
            {
                EndGame();
                return;
            }
        }

        private void YellowTeamIncrement()
        {
            yellowTeamDeaths++;
            YellowSegments--;
            Debug.Log(YellowSegments);
            if (yellowTeamDeaths <= 0)
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
            StopCoroutine(SpawnGunAfterTime());
            EndofGameEmpty.SetActive(true);
            Camera fpsCamera1 = _playerOne.GetComponent<FpController>().fpsCamera;
            Camera fpsCamera2 = _playerTwo.GetComponent<FpController>().fpsCamera;
            var stack1 = fpsCamera1.GetUniversalAdditionalCameraData();
            var stack2 = fpsCamera2.GetUniversalAdditionalCameraData();
            stack1.cameraStack.RemoveAt(1);
            stack2.cameraStack.RemoveAt(1);
            if (stack1 != null)
                stack1.cameraStack.Add(EndofGameCuties.GetComponent<Camera>());
            if (stack2 != null)
                stack2.cameraStack.Add(EndofGameCuties.GetComponent<Camera>());
            if (blueTeamDeaths >= 5) // Yellow Won
            {
                if (TeamManagerScript.Instance.PlayerOneTeam == 0) // player one is blue
                {
                    GameWinner.text = PhotonLauncher.Instance.GetOtherPlayerName(); // player two name
                    playerRender1.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                    playerRender2.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                }
                else // player one is yellow
                {
                    GameWinner.text = PhotonLauncher.Instance.GetHostName(); // player one (host) name
                    playerRender1.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                    playerRender2.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                }
            } else if (yellowTeamDeaths >= 5) // Blue Won
            {
                if (TeamManagerScript.Instance.PlayerOneTeam == 0) // player one is blue
                {
                    GameWinner.text = PhotonLauncher.Instance.GetHostName(); // player one (host) name
                    playerRender1.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                    playerRender2.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                }
                else // player one is yellow
                {
                    GameWinner.text = PhotonLauncher.Instance.GetOtherPlayerName(); // player two name
                    playerRender1.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                    playerRender2.material.color = CharacterColorChoices.ColorChoices[PhotonLauncher.Instance.GetPlayerTwoColor()];
                }
            }
            playerRender1.UpdateGIMaterials();
            playerRender2.UpdateGIMaterials();
            // set their dancing and dying animation
            EndofGameCutiesAnimators[0].SetBool(EndofGameCutiesAnimators[0].parameters[2].name, true);
            EndofGameCutiesAnimators[1].SetBool(EndofGameCutiesAnimators[1].parameters[3].name, true);
            EndofGameCuties.SetActive(true);
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

