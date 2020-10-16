using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

using VoltageSource;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameObject LocalPlayerInstance;

    private void Awake()
    {
        if(LocalPlayerInstance == null){
            if (photonView.IsMine)
            {
                LocalPlayerInstance = this.gameObject;
            }

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            
        }
    }


    // Handle being hit and store health. Best done by get and set funcitons
    [SerializeField] private float health;
    public float Health
    {
        get => health;
        set
        {
            if (health <= 0)
            {
                Debug.Log("Died");
                GameManager.Instance.LeaveRoom();
                // If player dies then all function to handle their death
            }
            health = value;
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Health);
        }
        else
        {
            Health = (float) stream.ReceiveNext();
        }
    }
}
