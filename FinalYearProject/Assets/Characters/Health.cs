using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Health : MonoBehaviour
{
    public float health = 100f;
    private Color[] originalColors;
    public Color hitColor = Color.red;
    // Method to apply damage to the target
    private Renderer[] arrayOfRenderers;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hitSound;       // Sound when entity is hit
    public AudioClip deathSound;     // Sound when entity dies

    void Start()
    {
        arrayOfRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[arrayOfRenderers.Length];
        if (arrayOfRenderers != null)
        {
            int i = 0;
            foreach (var renderer in arrayOfRenderers)
            {
                originalColors[i] = renderer.material.color;
                i++;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (this.enabled == false) return;
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        for (int i = 0; i < arrayOfRenderers.Length; i++)
        {
            arrayOfRenderers[i].material.color = hitColor;
        }
        Invoke("ResetColor", 0.5f);  // Resets color after 0.5 seconds

        health -= damage;
        if (health <= 0f)
        {
            Die();
        }
}

    // Method to handle the target's death
    private void Die()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        Debug.Log($"{gameObject.name} has died.");
        if (CompareTag("Player"))
        {
            GameTracker.instance.StopTimer();
            GameTracker.instance.SaveSession();


        }
        Destroy(gameObject);
        if (CompareTag("Enemy"))
        {
            GameTracker.instance.IncrementSlimeKilled();

        }
    }
    public void Suicide()
    {
        health = 0;
        Die();
    }


    private void ResetColor()
    {
        for (int i = 0; i < arrayOfRenderers.Length; i++)
        {
            arrayOfRenderers[i].material.color = originalColors[i];
        }
    }
}




