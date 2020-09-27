using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

using System.Collections;

[RequireComponent(typeof(TMP_InputField))]
public class PhotonPlayerNameInputScript : MonoBehaviour
{
    #region Private Constants

    private const string playerNamePrefKey = "Playername";

    #endregion

    #region MonoBehaviour CallBacks

    private void Start()
    {
        string defaultName = string.Empty;
        TMP_InputField _inputField = this.GetComponent<TMP_InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
            
        }

        PhotonNetwork.NickName = defaultName;
    }

    #endregion

    #region Public Methods

    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player name is null or empty");
            return;
        }

        PhotonNetwork.NickName = value;
        
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    #endregion
    
}
