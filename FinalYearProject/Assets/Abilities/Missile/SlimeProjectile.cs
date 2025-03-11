using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SlimeProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 5f; // Time after which the projectile is destroyed
    private float timer;

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy projectile after a certain time
        GetComponent<Rigidbody>().velocity = speed * transform.forward;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
        //transform.Translate(Vector3.forward * speed * Time.deltaTime);
     
    }

    private void OnTriggerEnter(Collider collider)
    {
        Health target = collider.transform.GetComponent<Health>();
 
        if (target != null && target.tag == "Player")
        {
            target.TakeDamage(damage);

            Destroy(gameObject);
        }


         // Destroy the projectile on impact
    }
}
