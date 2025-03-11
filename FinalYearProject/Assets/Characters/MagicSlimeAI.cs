using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;



public class MagicSlimeAI : SlimeAI
{
    public GameObject prefabBullet;
    public float inBetweenCastTime = 3f;
    public float Timer = 0f;
    public float AttackRange = 50f;


    public override void SlimeSpellUpdate()
    {

        Timer += Time.deltaTime;
        if (Vector3.Distance(transform.position, target.position) < AttackRange)
        {
            if (Timer >= inBetweenCastTime)
            {
                Instantiate(prefabBullet, transform.position, transform.rotation);
                Timer = 0f;
            }
            
        }

    }
  
}

