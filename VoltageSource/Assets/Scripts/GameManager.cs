using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

namespace VoltageSource
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;
        public GameObject playerPrefab;

        private void Start()
        {
            Instance = this;
            if (playerPrefab == null)
            {
                Debug.LogError("Missing player prefab on game manager");
            }
            else
            {
                if(FP_Controller.LocalPlayerInstance == null){
                    PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 5, 0), Quaternion.identity, 0);
                }
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

