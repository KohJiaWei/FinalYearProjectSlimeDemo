using System.Collections.Generic;
using UnityEngine;



public class LightningSpell : MonoBehaviour
{
    public int damage = 100;
    public float range = 100f;
    public Camera fpsCam; // Assuming you're using a first-person camera
    public LightningInstance lightningInstance;
    //void Update()
    //{
    //    //Debug.Log("Does this script even run");
    //    if (Input.GetButtonDown("Fire1"))
    //    {
    //        Debug.Log("UpdatePlayerBullet");
    //        Shoot();
    //    }
    //}
        public void Shoot()
    {
        RaycastHit[] hitMultiple;
        Vector3 origin = fpsCam.transform.position;
        Vector3 direction = fpsCam.transform.forward;
        var sphereRadius = 0.2f;
        float maxDistance = 999f;

        hitMultiple = Physics.SphereCastAll(origin, sphereRadius, direction, maxDistance);
        foreach (var hit in hitMultiple )
        {
            var healthScript = hit.collider.GetComponent<Health>();
            if (healthScript == null) {
                continue;
            }
            if (healthScript.gameObject == this.gameObject) //this.gameObject is me because im the instance that gameObject is on
            {
                continue;
            }
            healthScript.TakeDamage(damage);
            var lightningInstantiation = Instantiate(lightningInstance);
            lightningInstantiation.transform.position = origin;

            HashSet<Health> alreadyHit = new HashSet<Health>() {healthScript};

            lightningInstantiation.__init__(origin,healthScript.transform.position, alreadyHit);
        }
       
    }
}
