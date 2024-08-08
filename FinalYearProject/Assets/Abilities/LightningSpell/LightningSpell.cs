using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningSpell : MonoBehaviour
{
    public ParticleSystem lightningEffect;
    public LineRenderer lightningStrike;
    public AudioSource lightningSound;
    public Transform target;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) // Change the key as needed
        {
            CastLightning();
        }
    }

    void CastLightning()
    {
        // Play particle effect
        lightningEffect.Play();

        // Draw the line renderer
        lightningStrike.SetPosition(0, transform.position);
        lightningStrike.SetPosition(1, target.position);

        // Play the sound effect
        lightningSound.Play();

        // Optionally, add damage or other effects to the target
    }
}

