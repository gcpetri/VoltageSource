using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Pun.UtilityScripts;

[RequireComponent(typeof(PhotonTeamsManager))]
public class TeamManagerScript : MonoBehaviour
{
   public static TeamManagerScript Instance;
   private PhotonTeamsManager ptm;
   
   
   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
      }
      else
      {
         Destroy(this);
      }
      DontDestroyOnLoad(this);
      ptm = GetComponent<PhotonTeamsManager>();
   }
   
}
