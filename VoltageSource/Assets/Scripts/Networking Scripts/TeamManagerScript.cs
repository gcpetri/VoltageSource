using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;


public class TeamManagerScript : MonoBehaviour
{
   public static TeamManagerScript Instance;

   // Assume player one is always the master client (which should be the case)
   [SerializeField]private int _playerOneTeam = 0;
   [SerializeField]private int _playerTwoTeam = 1;
   public int PlayerOneTeam
   {
      get => _playerOneTeam;
      set
      {
         _playerOneTeam = value;
      }
   }
   public int PlayerTwoTeam
   {
      get => _playerTwoTeam;
      set
      {
         _playerTwoTeam = value;
      }
   }
   
   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
      }
      else
      {
         Destroy(this.gameObject);
      }
      DontDestroyOnLoad(this);
   }
   
}
