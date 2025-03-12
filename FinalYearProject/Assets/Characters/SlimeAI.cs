using UnityEngine;

public class SlimeAI : MonoBehaviour
{
    // Existing variables
    public float moveSpeed = 2f;
    public float damage = 10f;
    public Color hitColor = Color.red;
    public Transform target;

    // New separation variables
    public float separationDistance = 1.5f;
    public float separationForce = 2f;

    private Color originalColor;
    private Renderer rendererSlime;
    private Rigidbody rb;
    private Collider[] slimeColliders;
    protected Vector3 combinedDirection;
    public float SlimeAttackCooldown = 1f;
    public float SlimeAttackCooldownTimer = 0f;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        rendererSlime = GetComponentInChildren<Renderer>();

        if (rendererSlime != null)
        {
            originalColor = rendererSlime.material.color;
        }

        // Make sure collider is enabled
        GetComponent<Collider>().enabled = true;
    }

    void Update()
    {
        SlimeSpellUpdate();
        SlimeAttackCooldownTimer += Time.deltaTime;
        if (target != null)
        {
            // Calculate player direction
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;

            // Add separation from other slimes
            Vector3 separation = CalculateSeparation();
            combinedDirection = (horizontalDirection + separation).normalized;

            // Move with separation
            rb.MovePosition(transform.position + combinedDirection * moveSpeed * Time.deltaTime);

            // Rotate to face movement direction if moving
            if (combinedDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(combinedDirection);
                rb.MoveRotation(targetRotation);
            }
        }
    }

    Vector3 CalculateSeparation()
    {
        Vector3 separation = Vector3.zero;
        int count = 0;

        // Check for nearby slimes
        slimeColliders = Physics.OverlapSphere(transform.position, separationDistance);
        foreach (Collider col in slimeColliders)
        {
            if (col != null && col.CompareTag("Enemy") && col.gameObject != this.gameObject)
            {
                Vector3 toNeighbor = transform.position - col.transform.position;
                separation += toNeighbor.normalized / Mathf.Max(toNeighbor.magnitude, 0.1f);
                count++;
            }
        }

        if (count > 0)
        {
            separation /= count;
            separation = separation.normalized * separationForce;
        }

        return separation;
    }

    // Visualize separation distance in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (SlimeAttackCooldownTimer < SlimeAttackCooldown)
        {
            return;
        }
        
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Health>().TakeDamage(damage);
            collision.gameObject.GetComponent<PlayerMovement>().ApplyKnockback(transform.forward, 100);


        }
    }
    public virtual void SlimeSpellUpdate(){
        
       }

 }

