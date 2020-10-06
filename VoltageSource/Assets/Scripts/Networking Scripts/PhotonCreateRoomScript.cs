using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using VoltageSource;

[RequireComponent(typeof(TMP_InputField))]
public class PhotonCreateRoomScript : MonoBehaviour
{

    private string _roomName;

    private void Start()
    {
        _roomName = string.Empty;
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #region Public Methods

    
    public void SetRoomName(string value)
    {
        
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Room name is null or empty");
            return;
        }
        
        _roomName = value;
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(_roomName))
        {
            Debug.LogError("Room name is null or empty");
            return;
        }
        PhotonLauncher.Instance.CreateRoom(_roomName);
    }

   

    #endregion
    
}
