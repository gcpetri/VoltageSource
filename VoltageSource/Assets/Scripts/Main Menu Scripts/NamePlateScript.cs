using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using VoltageSource;
using UnityEngine.SceneManagement;

public class NamePlateScript : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private TMP_Text playerOnePlate;
    [SerializeField] private TMP_Text playerTwoPlate;
    

    private void Start()
    {
        if (playerOnePlate == null || playerTwoPlate == null)
            return;

        if(PhotonLauncher.Instance == null)
            Debug.LogErrorFormat("Photon Launcher is missing from scene index: {0}", SceneManager.GetActiveScene().buildIndex);
        playerOnePlate.text = PhotonLauncher.Instance.GetHostName();
    }


    #region PunCallbacks
    

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
        playerOnePlate.text = PhotonLauncher.Instance.GetHostName();
        playerTwoPlate.text = PhotonLauncher.Instance.GetOtherPlayerName();
        Debug.Log(PhotonLauncher.Instance.GetOtherPlayerName());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //playerOnePlate.text = "Player One";
        //playerTwoPlate.text = "Player Two";
        playerOnePlate.text = " ";
        playerTwoPlate.text = " ";
    }

    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerOnePlate.text);
            stream.SendNext(playerTwoPlate.text);
        }
        else
        {
            playerOnePlate.text = (string)stream.ReceiveNext();
            playerTwoPlate.text = (string)stream.ReceiveNext();
        }
    }
}
