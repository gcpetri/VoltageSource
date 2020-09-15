using UnityEngine;

public class FP_gunScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    
    public Camera FP_cam;
    public ParticleSystem laserShot;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
    void Shoot()
    {
        laserShot.Play();
        RaycastHit hit;
        if (Physics.Raycast(FP_cam.transform.position, FP_cam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
        }
    }
}
