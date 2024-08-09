using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class InvisibleBullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 100;
    public Rigidbody rb;

    void Start()
    {
        rb.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Slime"))
        {
            Health slime = other.GetComponent<Health>();
            if (slime != null)
            {
                slime.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}