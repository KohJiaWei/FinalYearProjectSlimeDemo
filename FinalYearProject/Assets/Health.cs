using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Health : MonoBehaviour
{
    public float health = 100f;
    private Color originalColor;
    public Color hitColor = Color.red;
    private Rigidbody rb;
    // Method to apply damage to the target
    private Renderer rendererSlime;

    void Start()
    {
        rendererSlime = GetComponentInChildren<Renderer>();
        // Save the original color
        if (rendererSlime != null)
        {
            originalColor = rendererSlime.material.color;
        }
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    // Method to handle the target's death
    private void Die()
    {
        // Here you can handle what happens when the target dies
        // For example, play a death animation, drop loot, etc.
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject); // Destroy the target GameObject
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rocket"))
        {
            // Change color
            if (rendererSlime != null)
            {
                rendererSlime.material.color = hitColor;
            }
            Invoke("ResetColor", 0.5f);  // Resets color after 0.5 seconds
        }
    }

    private void ResetColor()
    {
        if (rendererSlime != null)
        {
            rendererSlime.material.color = originalColor;
        }
    }
}

