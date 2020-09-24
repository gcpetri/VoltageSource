using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class FP_gunScript : MonoBehaviour
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
    public GameObject gunLight;
    // reloading
    private int _currentAmmo = -1;
    private bool _isReloading = false;
    // bullets
    [SerializeField][InspectorName("Gun Animator")]private Animator _animator;
    public Transform barrelEnd;
    // aim down sights
    public Vector3 aimHipFire;
    public Vector3 aimDownSights;
    public float aimSpeed = 10f;
    [Header("Sound Clips")]
    // gun sound
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioSource audioSource;
    // reload sound
    [SerializeField] private AudioClip reloadSound;

    //********************// Use ID based animator changes because it more efficient 

    private void Start()
    {
        if(_animator == null)
                _animator = GetComponent<Animator>();
        

        if (gunData == null)
            return;
        
        if (_currentAmmo == -1) 
            _currentAmmo = gunData.maxAmmo;
        // Using .GetComponent during start / awake isn't all that bad. But using it during update/fixed update/late update isn't good for performance
        _meshRender = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRender = gunData.mesh;
        _meshFilter = gunData.meshFilter;
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
            transform.localPosition = Vector3.Slerp(transform.localPosition, aimDownSights, aimSpeed * Time.deltaTime);
        } else
        {
            transform.localPosition = aimHipFire;
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
        gunLight.SetActive(true);
        RaycastHit hit;
        if (Physics.Raycast(FP_cam.transform.position, FP_cam.transform.forward, out hit, gunData.range))
        {
            //Debug.Log(hit.transform.name);
        }
        audioSource.PlayOneShot(gunSound, 0.05f);
        var instantiateBullet = Instantiate(gunData.bulletPrefab, barrelEnd.position, barrelEnd.transform.rotation);
        instantiateBullet.GetComponent<Rigidbody>().velocity = instantiateBullet.transform.right * gunData.bulletSpeed;
        Destroy(instantiateBullet, 2f);
        gunLight.SetActive(false);
    }
    private IEnumerator Reload()
    {
        _isReloading = true;
        //Debug.Log("reloading...");
        //_animator.SetBool(_reloadingID, true);
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(gunData.reloadTime - .25f);
        //_animator.SetBool(_reloadingID, false);
        yield return new WaitForSeconds(.25f);
        _currentAmmo = gunData.maxAmmo;
        _isReloading = false;
    }

    private void GunChange()
    {
        //change _gunData to the appropriate data for the gun that was changed
        // 
    }
}
