using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public Camera fpsCam;
    public GameObject projectilePrefab;
    public float shootForce = 1000f;
    public Animator anim;


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null)
        {
            anim.Play("Attack01",0,0);
            // Instantiate the projectile at the camera's position and rotation
            GameObject projectile = Instantiate(projectilePrefab, fpsCam.transform.position, fpsCam.transform.rotation);

            // Add force to the projectile
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(fpsCam.transform.forward * shootForce);
            }
        }
    }
}
