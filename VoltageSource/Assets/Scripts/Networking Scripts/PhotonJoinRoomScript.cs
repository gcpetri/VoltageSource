using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using VoltageSource;

[RequireComponent(typeof(TMP_InputField))]
public class PhotonJoinRoomScript : MonoBehaviour
{
    
    private string _roomName = String.Empty;

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

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(_roomName))
        {
            Debug.LogError("Room name is null or empty");
            return;
        }

        PhotonLauncher.Instance.JoinRoom(_roomName);
    }
    
    #endregion
    
}