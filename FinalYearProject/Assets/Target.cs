using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 100f;

    // Method to apply damage to the target
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
}

