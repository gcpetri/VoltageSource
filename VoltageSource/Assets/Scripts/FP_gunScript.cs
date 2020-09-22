using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class FP_gunScript : MonoBehaviour
{
    // gun specs
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 5f;
    private float nextTimetoFire = 0f;
    // camera
    public Camera FP_cam;
    public ParticleSystem laserShot;
    // reloading
    public int maxAmmo = 40;
    private int currentAmmo = -1;
    public float reloadTime = 1f;
    private bool isReloading = false;
    // bullets
    public Animator animator;
    public float bulletSpeed = 100f;
    public Transform barrelEnd;
    public GameObject bullet;
    // gun sound
    public AudioClip gunSound;
    public AudioSource audioSource;
    // reload sound
    public AudioClip reloadSound;

    private void Start()
    {
        bullet.SetActive(false);
        if (currentAmmo == -1) 
            currentAmmo = maxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        if (isReloading)
        {
            return;
        }
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetButton("Fire1") && Time.time >= nextTimetoFire)
        {
            nextTimetoFire = Time.time + 1 / fireRate;
            Shoot();
            Debug.Log("shot");
        }
    }
    void Shoot()
    {
        currentAmmo--;
        laserShot.Play();
        RaycastHit hit;
        if (Physics.Raycast(FP_cam.transform.position, FP_cam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
        }
        audioSource.PlayOneShot(gunSound);
        bullet.SetActive(true);
        var instantiateBullet = Instantiate(bullet, barrelEnd.position, barrelEnd.transform.rotation);
        instantiateBullet.GetComponent<Rigidbody>().velocity = instantiateBullet.transform.right * bulletSpeed;
        Destroy(instantiateBullet, 2f);
    }
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("reloading...");
        animator.SetBool("Reloading", true);
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(.25f);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
