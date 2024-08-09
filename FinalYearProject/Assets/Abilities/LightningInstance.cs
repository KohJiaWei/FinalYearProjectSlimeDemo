using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class LightningInstance : MonoBehaviour
{
    public LineRenderer lightningLineRenderer;
    public int damage = 50;
    public int chainLightningDistance = 5;


    public void __init__(Vector3 startPos, Vector3 endPos,HashSet<Health> alreadyHit)
    {
        lightningLineRenderer.SetPosition(0, startPos);
        lightningLineRenderer.SetPosition(1, endPos);
        var AllHealth = FindObjectsOfType<Health>();
        foreach (Health healthScript in AllHealth)
        {
            if (alreadyHit.Contains(healthScript))
            {
                continue;
            }
           
            //Debug.Log($"{Vector3.Distance(healthScript.transform.position,endPos)}vs{endPos}");
            if (Vector3.Distance(healthScript.transform.position, endPos) > chainLightningDistance)
            {
                continue;
            }

            healthScript.TakeDamage(damage);
            var lightningInstantiation = Instantiate(this);

            lightningInstantiation.transform.position = endPos;
            alreadyHit.Add(healthScript);

            lightningInstantiation.__init__(endPos, healthScript.transform.position, alreadyHit);
            break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
