using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using VoltageSource;

[RequireComponent(typeof(Animator))]
public class GunScript : MonoBehaviour
{
    [SerializeField] private GunsScriptable gunData;
    private float _nextTimeToFire = 0f;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private int _currentAmmo = -1;
    private bool _isReloading = false;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private Animator gunAnimator;
    [SerializeField]private ParticleSystem _gunParticleSystem;

    [Header("Gun Sound Clips")] 
    [SerializeField] private AudioClip firingSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reloadSound;

    private int _reloadingID;
    private int _aimingID;
    private int _shootingID;

    private bool _isAiming = false;
    public bool isAiming
    {
        get => _isAiming;
        set => _isAiming = value;
    }
    
    private void Start()
    {
        gunAnimator = GetComponent<Animator>();
        _reloadingID = Animator.StringToHash("Reloading");
        _aimingID = Animator.StringToHash("Aiming");
        _shootingID = Animator.StringToHash("Shooting");
        
        // Gun start with max ammo by default
        _currentAmmo = gunData.maxAmmo;
    }
    
    [PunRPC]
    public void Shoot(int owner)
    {
        if ((_currentAmmo <= 0 || _nextTimeToFire >= Time.time) || _isReloading) 
        {
            return;
        }
        gunAnimator.SetBool(_shootingID, true);
        _currentAmmo--;
        _gunParticleSystem.Play(); // tells data, but doens't have location
        audioSource.PlayOneShot(firingSound);
        var instantiateBullet = Instantiate(gunData.bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.transform.rotation);
        instantiateBullet.GetComponent<BulletScript>().damage = gunData.damage;
        instantiateBullet.GetComponent<Rigidbody>().velocity = instantiateBullet.transform.right * gunData.bulletSpeed;
        instantiateBullet.GetComponent<BulletScript>().Owner = owner;
        Destroy(instantiateBullet, Mathf.Clamp(gunData.range / (gunData.bulletSpeed), 0f, 10f));
        if (_currentAmmo <= 0)
        {
            StartReloading();
        }
        _nextTimeToFire = Time.time + (1 / gunData.firerate);
        gunAnimator.SetBool(_shootingID, false);
    }
    
    private IEnumerator Reload()
    {
        _isReloading = true;
        //Debug.Log("reloading...");
        gunAnimator.SetBool(_reloadingID, true);
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(gunData.reloadTime - .25f);
        gunAnimator.SetBool(_reloadingID, false);
        yield return new WaitForSeconds(.25f);
        _currentAmmo = gunData.maxAmmo;
        _isReloading = false;
    }

    public void StartReloading()
    {
        StartCoroutine(Reload());
    }

}
