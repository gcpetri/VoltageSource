using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private GameObject _decal;
    [HideInInspector] public float damage;
    private Rigidbody _rb;
    private Vector3 _lastPosition;
    [SerializeField] private LayerMask layermask;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _lastPosition = transform.position;
    }
    
    private void LateUpdate()
    {
        // Implement raycast each frame. 
        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - _lastPosition;
        RaycastHit hit;
        Debug.DrawRay(_lastPosition, direction, Color.green);
        if (Physics.Raycast(_lastPosition, direction, out hit, 100f, layermask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<FP_Controller>().Health = hit.collider.GetComponent<FP_Controller>().Health - damage; 
            }
            else
            {
                GameObject impact = GameObject.Instantiate(_decal, transform.position, Quaternion.identity);
                impact.transform.position = hit.point + (hit.normal * 0.01f);
                impact.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                Destroy(impact, 5f);
            }
            gameObject.SetActive(false);
            _lastPosition = transform.position;
            Destroy(this);
        }

        _lastPosition = transform.position;
    }
}
