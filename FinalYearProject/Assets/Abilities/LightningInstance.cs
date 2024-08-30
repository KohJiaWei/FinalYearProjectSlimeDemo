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
    public AudioClip LightningAudio;
    public static int AmountOfLightningInTheWorld;
    public int MaxAmountOfLightningInTheWorld = 3;
    private List<Health> SlimeList = new List<Health>();


    public void __init__(Vector3 startPos, Vector3 endPos,HashSet<Health> alreadyHit)
    {
        lightningLineRenderer.SetPosition(0, startPos);
        lightningLineRenderer.SetPosition(1, endPos);
        var AllHealth = FindObjectsOfType<Health>();
        if (AmountOfLightningInTheWorld < MaxAmountOfLightningInTheWorld)
        {
            AudioSource.PlayClipAtPoint(LightningAudio, startPos);
        }
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

            SlimeList.Add(healthScript);
            
        }
        float CurrentClosestSlimeDistance = float.MaxValue;
        Health closestSlimeTarget = null;
        foreach (Health distanceCloseEnoughHealthScript in SlimeList)
        {
            float distance = Vector3.Distance(distanceCloseEnoughHealthScript.transform.position, endPos);
            if (distance < CurrentClosestSlimeDistance)
            {
                CurrentClosestSlimeDistance = distance;
                closestSlimeTarget = distanceCloseEnoughHealthScript;
            }
            
        }
        closestSlimeTarget.TakeDamage(damage);
        var lightningInstantiation = Instantiate(this);

        lightningInstantiation.transform.position = endPos;
        alreadyHit.Add(closestSlimeTarget);

        lightningInstantiation.__init__(endPos, closestSlimeTarget.transform.position, alreadyHit);
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

    private void OnDestroy()
    {
        AmountOfLightningInTheWorld--;  
    }

    private void Awake()
    {
        AmountOfLightningInTheWorld++;

    }


}
