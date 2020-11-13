#define DEBUG
#undef  DEBUG

using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class GunScript : MonoBehaviour
{
    [SerializeField] public GunsScriptable gunData;
    private float _nextTimeToFire = 0f;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    public int _currentAmmo = -1;
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
    [SerializeField] private GameObject gunCamera;
    [SerializeField] private FpController fpController;

    [SerializeField] public Slider ammoSlider;
    private int _ownerPhotonID;

    
    private bool _hasAnOwner = false;
    [SerializeField] private SphereCollider sphereCollider;
    private Transform _thisTransform;
    [SerializeField] private PhotonAnimatorView photonAnimatorView;

    private float _spreadRatio;
    private PhotonView ownerPhotonView = null;

    private int _reloadingID;
    private int _aimingID;
    private int _shootingID;
    private int _rotatingID;
    private bool _isAiming = false;
    
    private Vector2 _spread;
    private Ray _ray;
    private Vector3 _hitpos;
    
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
        if (photonAnimatorView)
        {
            photonAnimatorView.enabled = false;
        }
        
        // Gun start with max ammo by default
        _currentAmmo = gunData.maxAmmo;
        if (ammoSlider)
        {
            ammoSlider.maxValue = gunData.maxAmmo;
            ammoSlider.value = gunData.maxAmmo;
        }
        
        if (audioSource)
        {
            audioSource.volume = PlayerPrefs.GetFloat("VolumeValue");
        }

        if (_hasAnOwner)
        {
            sphereCollider.enabled = false;
            _spreadRatio = gunData.spreadAngle / fpController.fpsCamera.fieldOfView;
            gameObject.layer = LayerMask.NameToLayer("Gun");
            if (gunAnimator)
            {
                gunAnimator.SetBool(_rotatingID, false);
            }
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
        
        _spread = _spreadRatio * Random.insideUnitCircle;
        Vector3 vect = Vector3.one;
        vect.x *= 0.5f; vect.y *= 0.505f; vect.z *= 0.5f; // Kinda Guess and Check (distance matters)
        _ray = fpController.fpsCamera.ViewportPointToRay(vect + (Vector3) _spread);
        _hitpos = _ray.origin + _ray.direction * 200f;
        _currentAmmo--;
        ammoSlider.maxValue = gunData.maxAmmo;
        ammoSlider.value -= 1;
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
        if(_gunParticleSystem) // Works on multiplayer because of shared actions 
            _gunParticleSystem.Play(); // tells data, but doens't have location
        
        if(firingSound) // Play the gunshot sound for the character (doesn't work on multiplayer right now)
            audioSource.PlayOneShot(firingSound);
        
        var instantiateBullet = Instantiate(gunData.bulletPrefab, bulletSpawnPoint.position, transform.rotation);
        if (gunData.gunIndex == 0)
            instantiateBullet.transform.Rotate(new Vector3(0.0f, 90.0f, 90.0f));
        //Debug.LogFormat("Called ShareActions() on object {0} with photonView {1}", gameObject.name, _ownerPhotonID);
        BulletScript bulletScript = instantiateBullet.GetComponent<BulletScript>();
        bulletScript.damage = gunData.damage;
        instantiateBullet.GetComponent<Rigidbody>().velocity = _ray.direction * gunData.bulletSpeed;
        bulletScript.owner = _ownerPhotonID;
        bulletScript.lifeTime = Mathf.Clamp(gunData.range / (gunData.bulletSpeed), 0f, 10f);
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
        ammoSlider.maxValue = gunData.maxAmmo;
        ammoSlider.value = ammoSlider.maxValue;
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
        if (!gunCamera)
        {
            Debug.LogWarning("Missing camera reference");
            return;
        }
            
        
        gunCamera.transform.position = cameraIdle.position;
    }

    public void SetCameraAimPose()
    {
        if (!gunCamera)
        {
            Debug.LogWarning("Missing camera reference");
            return;
        }
        
        gunCamera.transform.position = cameraAim.position;
    }

    public GameObject GetCamera()
    {
        return gunCamera;
    }

    public void SetOwner(FpController obj)
    {
        fpController = obj;
        _hasAnOwner = true;
        sphereCollider.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Gun");
        if (gunAnimator)
        {
            gunAnimator.SetBool(_rotatingID, false);
        }

        photonAnimatorView.enabled = true;
        _ownerPhotonID = obj.photonView.ViewID;
        _spreadRatio = gunData.spreadAngle / this.fpController.fpsCamera.fieldOfView;
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
