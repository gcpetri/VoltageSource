using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class Pickupable : MonoBehaviour
{
    public void PickupObject()
    {
        Debug.Log("Picked up");
    }
}
