using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoltageSource;

namespace Main_Menu_Scripts
{
    public class NamePlateScript : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private TMP_Text playerOnePlate;
        [SerializeField] private TMP_Text playerTwoPlate;

        private byte UPDATENAMEPLATE = 2;

        private void Start()
        {
            if (playerOnePlate == null || playerTwoPlate == null)
                return;

            if(PhotonLauncher.Instance == null)
                Debug.LogErrorFormat("Photon Launcher is missing from scene index: {0}", SceneManager.GetActiveScene().buildIndex);
            playerOnePlate.text = PhotonLauncher.Instance.GetHostName();
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #region PunCallbacks
    

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            string playerOneName = PhotonLauncher.Instance.GetHostName();
            string playerTwoName = PhotonLauncher.Instance.GetOtherPlayerName();
            object[] content = new object[] {playerOneName, playerTwoName}; // Array contains the target position and the IDs of the selected units
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(UPDATENAMEPLATE, content, raiseEventOptions, SendOptions.SendReliable);
        
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
        
            string playerOneName = PhotonLauncher.Instance.GetHostName();
            string playerTwoName = PhotonLauncher.Instance.GetOtherPlayerName();
            object[] content;
            if (otherPlayer.NickName == playerOneName)
            {
                content = new object[] {"Player One", playerTwoName};
            }
            else
            {
                content = new object[] {playerOneName, "Player Two"};
            }
        
            // Array contains the target position and the IDs of the selected units
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(UPDATENAMEPLATE, content, raiseEventOptions, SendOptions.SendReliable);
        }

        #endregion
    
        public void OnEvent(EventData photonEvent)
        {
            byte eventcode = photonEvent.Code;

            if (eventcode == UPDATENAMEPLATE)
            {
                object[] data = (object[]) photonEvent.CustomData;
                playerOnePlate.text = (string) data[0];
                playerTwoPlate.text = (string) data[1];
            }
        }
    }
}
