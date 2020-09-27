using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupChecker : MonoBehaviour
{
    private Camera _camera;
    public LayerMask _pickupMask;
    [SerializeField] private float pickUpDistance = 5f;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        // Do a short wave raycast to check for there
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, pickUpDistance, _pickupMask)){
            if (hit.distance <= pickUpDistance)
            {
                hit.transform.GetComponent<Pickupable>().PickupObject();
            }
        }
    }
}
