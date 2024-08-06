using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 5f; // Time after which the projectile is destroyed

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy projectile after a certain time
        GetComponent<Rigidbody>().velocity = speed * transform.forward;
    }

    void Update()
    {
        //transform.Translate(Vector3.forward * speed * Time.deltaTime);
     
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hello");
        Target target = collision.transform.GetComponent<Target>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
        Destroy(gameObject); // Destroy the projectile on impact
    }
}
