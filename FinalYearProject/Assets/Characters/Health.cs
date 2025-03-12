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

    void Start()
    {
        arrayOfRenderers = GetComponentsInChildren<Renderer>();
        // Save the original color
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
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
        if (CompareTag("Enemy"))
        {
            GameTracker.instance.incrementSlimeKilled();
        }
    }

    private void ResetColor()
    {
        for (int i = 0; i < arrayOfRenderers.Length; i++)
        {
            arrayOfRenderers[i].material.color = originalColors[i];
        }
    }
}

