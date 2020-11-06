#define DEBUG
#undef  DEBUG

using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class GunScript : MonoBehaviour
{
    [SerializeField] public GunsScriptable gunData;
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
    [SerializeField] private FpController fpController;
    private int ownerPhotonID;


    [SerializeField] private float rotatingSpeed = 200f;
    private bool _hasAnOwner = false;
    [SerializeField] private SphereCollider collider;
    private Transform _thisTransform;
    [SerializeField] private PhotonAnimatorView photonAnimatorView;

    private float spreadRatio;
    private PhotonView ownerPhotonView = null;
    private PhotonView _photonView;

    private int _reloadingID;
    private int _aimingID;
    private int _shootingID;
    private int _rotatingID;
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
        _thisTransform = transform;
        _photonView = GetComponent<PhotonView>();
        if (photonAnimatorView)
        {
            photonAnimatorView.enabled = false;
        }
        
        // Gun start with max ammo by default
        _currentAmmo = gunData.maxAmmo;

        #if DEBUG
            Debug.LogFormat("Animator exists: {0} on {1}", (gunAnimator != null), gameObject.name);
            Debug.LogFormat("GunData exists: {0} on {1}", (gunData != null), gameObject.name);
            Debug.LogFormat("AudioSource exists: {0} on {1}", (audioSource != null), gameObject.name);
        #endif
        if (audioSource)
        {
            audioSource.volume = PlayerPrefs.GetFloat("VolumeValue");
        }

        if (_hasAnOwner)
        {
            collider.enabled = false;
            spreadRatio = gunData.spreadAngle / this.fpController.fpsCamera.fieldOfView;
        }

        if (gunAnimator)
        {
            _reloadingID = Animator.StringToHash("Reloading");
            _aimingID = Animator.StringToHash("Aiming");
            _shootingID = Animator.StringToHash("Shooting");
            _rotatingID = Animator.StringToHash("Rotating");
            gunAnimator.SetBool(_rotatingID, true);
        }
    }

    private Vector2 spread;
    private Ray r;
    private Vector3 hitpos;
    
    public bool Shoot() // This just now call local changes to the script
    {
        if ((_nextTimeToFire >= Time.time) || _isReloading) 
        {
            return false;
        }
        if (_currentAmmo <= 0)
        {
            StartReloading();
            return false;
        }
        
        spread = spreadRatio * Random.insideUnitCircle;
        r = fpController.fpsCamera.ViewportPointToRay((Vector3.one * 0.5f) + (Vector3) spread);
        hitpos = r.origin + r.direction * 200f;
        _currentAmmo--;
        gunAnimator.SetBool(_shootingID,true);

        if (_currentAmmo <= 0)
        {
            StartReloading();
        }
        _nextTimeToFire = Time.time + (1 / gunData.firerate);
        return true;
    }

    /// <summary>
    /// These are actions shared by all clients that need to be called locally 
    /// </summary>
    public void SharedActions() // This calls the info that should be shared with all players
    {
        _gunParticleSystem.Play(); // tells data, but doens't have location
        audioSource.PlayOneShot(firingSound);
        var instantiateBullet = Instantiate(gunData.bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.transform.rotation);
        Debug.LogFormat("Called ShareActions() on object {0} with photonView {1}", gameObject.name, ownerPhotonID);
        BulletScript bulletScript = instantiateBullet.GetComponent<BulletScript>();
        bulletScript.damage = gunData.damage;
        instantiateBullet.GetComponent<Rigidbody>().velocity = r.direction * gunData.bulletSpeed;
        bulletScript.Owner = ownerPhotonID;
        Destroy(instantiateBullet, Mathf.Clamp(gunData.range / (gunData.bulletSpeed), 0f, 10f));
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
        if (!camera)
        {
            Debug.LogWarning("Missing camera reference");
            return;
        }
            
        
        camera.transform.position = cameraIdle.position;
    }

    public void SetCameraAimPose()
    {
        if (!camera)
        {
            Debug.LogWarning("Missing camera reference");
            return;
        }
        
        camera.transform.position = cameraAim.position;
    }

    public GameObject GetCamera()
    {
        return camera;
    }

    public void SetOwner(FpController obj)
    {
        fpController = obj;
        _hasAnOwner = true;
        collider.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Gun");
        if (gunAnimator)
        {
            gunAnimator.SetBool(_rotatingID, false);
        }

        photonAnimatorView.enabled = true;
        ownerPhotonID = obj.photonView.ViewID;
        spreadRatio = gunData.spreadAngle / this.fpController.fpsCamera.fieldOfView;
        ownerPhotonView = obj.photonView;
    }

    private void Update()
    {
        if (gunAnimator)
        {
            if (_hasAnOwner && gunAnimator.GetBool(_rotatingID))
            {
                gunAnimator.SetBool(_rotatingID, false);
            }
        }
    }
}
