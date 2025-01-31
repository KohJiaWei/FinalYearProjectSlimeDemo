using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("References")]
    public Camera fpsCam;
    public GameObject projectilePrefab;
    public Animator anim;
    public LightningSpell LightningSpell;

    [Header("Shooting Settings")]
    public float shootForce = 1000f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Lightning spell override
        Keyboard keyboard = Keyboard.current;

        if (Input.GetKey(KeyCode.V))
        {
            LightningSpell.Shoot();
            return;
        }

        // Regular projectile attack
        if (projectilePrefab == null) return;

        anim.Play("Attack01", 0, 0);

        // Instantiate projectile at the camera's position and rotation
        GameObject projectile = Instantiate(projectilePrefab, fpsCam.transform.position, fpsCam.transform.rotation);

        // Apply force if projectile has a Rigidbody
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(fpsCam.transform.forward * shootForce);
        }
    }
}
