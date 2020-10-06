using UnityEngine;
using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine.Audio;

public class FP_gunScript : MonoBehaviourPunCallbacks, IPunObservable
{
    // gun specs
    [Header("Gunspecs")] 
    [SerializeField] private GunsScriptable gunData;
    private float _nextTimetoFire = 0f;

    private MeshRenderer _meshRender;
    private MeshFilter _meshFilter;
    // camera
    public Camera FP_cam;
    // shoot effect
    public ParticleSystem laserShot;
    // reloading
    private int _currentAmmo = -1;
    private bool _isReloading = false;
    // bullets
    [SerializeField][InspectorName("Gun Animator")]private Animator _animator;
    public Transform barrelEnd;

    [Header("Sound Clips")]
    // gun sound
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioSource audioSource;
    // reload sound
    [SerializeField] private AudioClip reloadSound;

    //********************// Use ID based animator changes because it more efficient 
    // Animator ID's: 
    private int _reloadingID;
    private int _aimingID;

    private void Start()
    {
        if(_animator == null)
                _animator = GetComponent<Animator>();
        
        _reloadingID = Animator.StringToHash("Reloading");
        _aimingID = Animator.StringToHash("Aiming");

        if (gunData == null)
            return;
        
        if (_currentAmmo == -1) 
            _currentAmmo = gunData.maxAmmo;
        // Using .GetComponent during start / awake isn't all that bad. But using it during update/fixed update/late update isn't good for performance
        _meshRender = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        //_meshRender = gunData.mesh;
        //_meshFilter = gunData.meshFilter;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (_isReloading)
        {
            return;
        }
        if (_currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetButton("Fire2") && !_isReloading)
        {
            _animator.SetBool(_aimingID, true);
        } else
        {
            _animator.SetBool(_aimingID, false);
            
        }
        
        if (Input.GetButton("Fire1") && Time.time >= _nextTimetoFire)
        {
            _nextTimetoFire = Time.time + 1 / gunData.firerate;
            Shoot();
            //Debug.Log("fire");
        }
    }
    private void Shoot()
    {
        _currentAmmo--;
        laserShot.Play();
        audioSource.PlayOneShot(gunSound);
        var instantiateBullet = Instantiate(gunData.bulletPrefab, barrelEnd.position, barrelEnd.transform.rotation);
        instantiateBullet.GetComponent<BulletScript>().damage = gunData.damage;
        instantiateBullet.GetComponent<Rigidbody>().velocity = instantiateBullet.transform.right * gunData.bulletSpeed;
        Destroy(instantiateBullet, Mathf.Clamp(gunData.range / (gunData.bulletSpeed), 0f, 10f));
    }
    private IEnumerator Reload()
    {
        _isReloading = true;
        //Debug.Log("reloading...");
        _animator.SetBool(_reloadingID, true);
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(gunData.reloadTime - .25f);
        _animator.SetBool(_reloadingID, false);
        yield return new WaitForSeconds(.25f);
        _currentAmmo = gunData.maxAmmo;
        _isReloading = false;
    }

    private void GunChange()
    {
        //change _gunData to the appropriate data for the gun that was changed
        // 
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_isReloading);
        }
        else
        {
            _isReloading = (bool) stream.ReceiveNext();
        }
    }
}
