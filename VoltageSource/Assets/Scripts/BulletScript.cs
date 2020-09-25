using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private GameObject _decal;
    private void OnCollisionStay(Collision other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            GameObject impact = GameObject.Instantiate(_decal, transform.position, Quaternion.identity);
            Debug.DrawRay(other.contacts[0].point,other.contacts[0].normal, Color.red);
            impact.transform.position = other.contacts[0].point + (other.contacts[0].normal * 0.01f);
            impact.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, other.contacts[0].normal);
            Destroy(impact, 5f);
        }
        gameObject.SetActive(false);
        Destroy(this, 0.01f);
    }
}
