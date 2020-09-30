using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using VoltageSource;


public class MainMenuScript : MonoBehaviour
{

    [SerializeField] private GameObject matchMakingPanel;
    [SerializeField] private GameObject mainMenuPanel;

    
    public void MatchMakingButton()
    {
        mainMenuPanel.SetActive(false);
        matchMakingPanel.SetActive(true);
        // To ensure is connected to master, if not then we won't be able to join rooms or stuff
        PhotonLauncher.Instance.Connect();
    }

    public void SettingsButton()
    {
        Debug.Log("Setting button on MainMenu pressed");
    }

    public void QuitGameButton()
    {
        Debug.Log("Quit Game button pressed");
        Application.Quit();
    }

    public void BackButton()
    {
        mainMenuPanel.SetActive(true);
        matchMakingPanel.SetActive(false);
    }
    
}
