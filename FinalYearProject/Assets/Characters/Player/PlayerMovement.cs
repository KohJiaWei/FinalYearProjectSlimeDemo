using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Existing variables
    public CharacterController controller;
    public Transform playerBody;
    public float walkSpeed = 5.0f;
    public float runSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public Animator anim;

    // Knockback variables
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;

    

    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private Vector3 knockbackDirection;
    private float knockbackTimer;


    void Update()
    {
        // Handle knockback first
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            ApplyKnockbackMovement();
            ApplyGravity();
            return; // Skip other movement during knockback
        }

        // Existing ground check
        if (isGrounded == false && controller.isGrounded == true)
        {
            anim.Play("JumpEnd");
            isJumping = false;
        }
        isGrounded = controller.isGrounded;

        // Get movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Move relative to the player's forward direction
        Vector3 move = playerBody.right * x + playerBody.forward * z;
        move.Normalize();

        // Determine speed
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Animation handling
        if (!isJumping)
        {
            if (z > 0) anim.Play("BattleWalkForward");
            else if (z < 0) anim.Play("BattleWalkBack");
            else if (x > 0) anim.Play("BattleWalkRight");
            else if (x < 0) anim.Play("BattleWalkLeft");
        }

        // Update animation speed
        //if (anim != null)
        //{
        //    anim.SetFloat("Speed", move.magnitude);
        //}

        // Jumping logic
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && knockbackTimer <= 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.Play("JumpStart");
            isJumping = true;
        }

        ApplyGravity();

        if (Input.GetKeyDown(KeyCode.Minus)) 
        {
            Health playerHealth = GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Suicide();
                Debug.Log("Player instantly killed!");
            }
        }


    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ApplyKnockbackMovement()
    {
        // Apply knockback direction with damping
        float damping = 1 - (knockbackTimer / knockbackDuration);
        Vector3 knockback = knockbackDirection * knockbackForce * damping * Time.deltaTime;
        controller.Move(knockback);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        // Normalize and store knockback direction
        knockbackDirection = direction.normalized;
        knockbackForce = force;
        knockbackTimer = knockbackDuration;

        // Reset vertical velocity for better knockback feel
        velocity.y = 0;

        // Trigger hit animation
        anim.SetTrigger("IsHit");
    }
}