using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gun Scriptable")]
public class GunsScriptable : ScriptableObject
{
    [SerializeField] public float damage;
    public float range;
    public float firerate;
    public int maxAmmo;
    public float bulletSpeed;
    public float reloadTime;
    public GameObject bulletPrefab;
    public float spreadAngle;
}
