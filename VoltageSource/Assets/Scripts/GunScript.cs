#define DEBUG
#undef  DEBUG

using System.Collections;
using Photon.Pun;
using UnityEngine;

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

    [Header("Gun Camera Aim")] [SerializeField]
    private Transform cameraIdle;

    [SerializeField] private Transform cameraAim;
    [SerializeField] private GameObject camera;
    [SerializeField] private FpController owner;
    [SerializeField] private LayerMask collisionMask;
    
    private int _reloadingID;
    private int _aimingID;
    private int _shootingID;
    private bool _isAiming = false;
    public bool isAiming
    {
        get => _isAiming;
        set
        {
            gunAnimator.SetBool(_aimingID, value);
            _isAiming = value;
        } 
    }
    
    private void Start()
    {
        gunAnimator = GetComponent<Animator>();
        _reloadingID = Animator.StringToHash("Reloading");
        _aimingID = Animator.StringToHash("Aiming");
        _shootingID = Animator.StringToHash("Shooting");
        
        // Gun start with max ammo by default
        _currentAmmo = gunData.maxAmmo;

        #if DEBUG
            Debug.LogFormat("Animator exists: {0} on {1}", (gunAnimator != null), gameObject.name);
            Debug.LogFormat("GunData exists: {0} on {1}", (gunData != null), gameObject.name);
            Debug.LogFormat("AudioSource exists: {0} on {1}", (audioSource != null), gameObject.name);
        #endif
        
    }
    
    [PunRPC]
    public void Shoot(int owner)
    {
        #if DEBUG
            Debug.LogFormat("MaxAmmo: {0}", gunData.maxAmmo);
            Debug.LogFormat("CurrentAmmo: {0}", _currentAmmo);
            Debug.LogFormat("NextTimeToFire: {0}", _nextTimeToFire);
            Debug.LogFormat("IsReloading: {0}", _isReloading);
        #endif
        if ((_nextTimeToFire >= Time.time) || _isReloading) 
        {
            return;
        }
        if (_currentAmmo <= 0)
        {
            StartReloading();
            return;
        }

        var spreadRatio = gunData.spreadAngle / this.owner.fpsCamera.fieldOfView;
        Vector2 spread = spreadRatio * Random.insideUnitCircle;
        var r = this.owner.fpsCamera.ViewportPointToRay(Vector3.one * 0.5f + (Vector3) spread);
        Vector3 hitpos = r.origin + r.direction * 200f;
        Debug.DrawRay(r.origin, r.direction, Color.red);
        // What I want to do is send a ray out from the cam
        
        gunAnimator.SetBool(_shootingID, true);
        _currentAmmo--;
        _gunParticleSystem.Play(); // tells data, but doens't have location
        audioSource.PlayOneShot(firingSound);
        var instantiateBullet = Instantiate(gunData.bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.transform.rotation);
        instantiateBullet.GetComponent<BulletScript>().damage = gunData.damage;
        instantiateBullet.GetComponent<Rigidbody>().velocity = r.direction * gunData.bulletSpeed;
        instantiateBullet.GetComponent<BulletScript>().Owner = owner;
        Destroy(instantiateBullet, Mathf.Clamp(gunData.range / (gunData.bulletSpeed), 0f, 10f));
        if (_currentAmmo <= 0)
        {
            StartReloading();
        }
        _nextTimeToFire = Time.time + (1 / gunData.firerate);
    }
    
    private IEnumerator Reload()
    {
        #if DEBUG
            Debug.Log("Reload() IEnumerator called in GunScript");
        #endif
        _isReloading = true;
        gunAnimator.SetBool(_reloadingID, true);
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(gunData.reloadTime - .25f);
        gunAnimator.SetBool(_reloadingID, false);
        yield return new WaitForSeconds(.25f);
        _currentAmmo = gunData.maxAmmo;
        _isReloading = false;
    }

    private void StartReloading()
    {
        StartCoroutine(Reload());
    }

    public float GetFireRate()
    {
        return gunData.firerate;
    }

    public void SetCameraIdlePos()
    {
        camera.transform.position = cameraIdle.position;
    }

    public void SetCameraAimPose()
    {
        camera.transform.position = cameraAim.position;
    }

    public GameObject GetCamera()
    {
        return camera;
    }

    public void SetOwner(FpController obj)
    {
        owner = obj;
    }
    
    
}
