using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float breakTime = 4f; // Time before it falls
    public float respawnTime = 6f;
    private Vector3 initialPosition;
    private Rigidbody rb;
    public Collider contactCollider;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Keep platform static
        initialPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("create a debulog oncolisionenter");
        if (other.gameObject.CompareTag("Player")) // Make sure your player has the "Player" tag
        {
            Invoke("BreakPlatform", breakTime);
        }
    }

    void BreakPlatform()
    {
        rb.isKinematic = false; // Enable physics, so it falls
        Invoke("ResetPlatform", respawnTime);
        contactCollider.enabled = false;

    }

    void ResetPlatform()
    {
        Debug.Log("i hope its called");
        rb.isKinematic = true;
        rb.velocity = Vector3.zero; // Stop movement
        rb.angularVelocity = Vector3.zero; // Stop rotation
        transform.position = initialPosition;
        contactCollider.enabled = true;
    }

    
}