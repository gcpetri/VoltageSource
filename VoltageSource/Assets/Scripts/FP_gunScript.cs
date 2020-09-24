using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Audio;

public class FP_gunScript : MonoBehaviour
{
    // gun specs
    [Header("Gunspecs")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 5f;
    private float _nextTimetoFire = 0f;
    // camera
    public Camera FP_cam;
    // shoot effect
    public ParticleSystem laserShot;
    // reloading
    public int maxAmmo = 40;
    private int _currentAmmo = -1;
    public float reloadTime = 1f;
    private bool _isReloading = false;
    // bullets
    [SerializeField][InspectorName("Gun Animator")]private Animator _animator;
    public float bulletSpeed = 100f;
    public Transform barrelEnd;
    public GameObject bullet;
    
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
        if (_currentAmmo == -1) 
            _currentAmmo = maxAmmo;
        // Using .GetComponent during start / awake isn't all that bad. But using it during update/fixed update/late update isn't good for performance
        if(_animator == null)
            _animator = GetComponent<Animator>();

        _reloadingID = Animator.StringToHash("Reloading");
        _aimingID = Animator.StringToHash("Aiming");
    }

    // Update is called once per frame
    void Update()
    {
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
            _nextTimetoFire = Time.time + 1 / fireRate;
            Shoot();
            //Debug.Log("fire");
        }
    }
    private void Shoot()
    {
        _currentAmmo--;
        laserShot.Play();
        RaycastHit hit;
        if (Physics.Raycast(FP_cam.transform.position, FP_cam.transform.forward, out hit, range))
        {
            //Debug.Log(hit.transform.name);
        }
        audioSource.PlayOneShot(gunSound);
        bullet.SetActive(true);
        var instantiateBullet = Instantiate(bullet, barrelEnd.position, barrelEnd.transform.rotation);
        instantiateBullet.GetComponent<Rigidbody>().velocity = instantiateBullet.transform.right * bulletSpeed;
        Destroy(instantiateBullet, 2f);
    }
    private IEnumerator Reload()
    {
        _isReloading = true;
        //Debug.Log("reloading...");
        _animator.SetBool(_reloadingID, true);
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadTime - .25f);
        _animator.SetBool(_reloadingID, false);
        yield return new WaitForSeconds(.25f);
        _currentAmmo = maxAmmo;
        _isReloading = false;
    }
}
