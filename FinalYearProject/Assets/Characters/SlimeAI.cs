using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SlimeAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float damage = 10f;
    public float lifetime = 100f;
    public Color hitColor = Color.red;  // Color to change to when hit
    public Transform target;

    private Color originalColor;  // To store the original color
    private Renderer rendererSlime;
    private Rigidbody rb;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        //rendererSlime = GetComponentInChildren<Renderer>();

        // Save the original color
        if (rendererSlime != null)
        {
            originalColor = rendererSlime.material.color;
        }
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;

            // Move the slime towards the target
            rb.MovePosition(transform.position + horizontalDirection * moveSpeed * Time.deltaTime);

            // Rotate the slime to face the target horizontally
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            rb.MoveRotation(targetRotation);
        }
    }


}

