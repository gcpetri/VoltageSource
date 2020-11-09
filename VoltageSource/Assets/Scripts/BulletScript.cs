﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private GameObject decal;
    [HideInInspector] public float damage;
    private Rigidbody _rb;
    private Vector3 _lastPosition;
    [SerializeField] private LayerMask layermask;
    [HideInInspector] public int owner;
    [HideInInspector] public float lifeTime;
    private RaycastHit _hit;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _lastPosition = transform.position;
        Destroy(this, lifeTime);
    }
    
    private void LateUpdate()
    {
        // Implement raycast each frame. 
        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - _lastPosition;
        Debug.DrawRay(_lastPosition, direction, Color.green);
        if (Physics.Raycast(_lastPosition, direction, out _hit, 100f, layermask))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                var val = _hit.collider.GetComponent<PhotonView>().ViewID;
                if (val == owner)
                    return;
                            
                _hit.collider.gameObject.GetComponent<FpController>().SendMessage("IGotShot", damage, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                GameObject impact = Instantiate(decal, transform.position, Quaternion.identity);
                impact.transform.position = _hit.point + (_hit.normal * 0.01f);
                impact.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, _hit.normal);
                impact.transform.localScale = impact.transform.lossyScale * 0.25f;
                Destroy(impact, 5f);
            }
            gameObject.SetActive(false);
            Destroy(this);
        }

        _lastPosition = transform.position;
        
    }
}
