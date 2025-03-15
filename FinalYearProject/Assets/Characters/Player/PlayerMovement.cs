using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // ------------------------------
    // EXISTING VARIABLES
    // ------------------------------
    public CharacterController controller;
    public Transform playerBody;
    public float walkSpeed = 5.0f;
    public float runSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public Animator anim;

    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private Vector3 knockbackDirection;
    private float knockbackTimer;

    // ------------------------------
    // NEW AUDIO VARIABLES
    // ------------------------------
    [Header("Audio")]
    public AudioSource sfxAudioSource;        // For jump, running, etc.
    public AudioClip jumpClip;
    public AudioClip runClip;

    public AudioSource backgroundAudioSource; // For background music
    public AudioClip backgroundMusic;

    private bool isPlayingRunSound;
    // ------------------------------

    void Start()
    {
        // OPTIONAL: Play background music if not playing already
        if (backgroundAudioSource != null && backgroundMusic != null)
        {
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.loop = true;
            backgroundAudioSource.Play();
        }
    }

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
        if (!isGrounded && controller.isGrounded)
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
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Animation handling
        if (!isJumping)
        {
            // Basic directional animations
            if (z > 0) anim.Play("BattleWalkForward");
            else if (z < 0) anim.Play("BattleWalkBack");
            else if (x > 0) anim.Play("BattleWalkRight");
            else if (x < 0) anim.Play("BattleWalkLeft");
        }

        // --- NEW: Footstep / Running sound ---
        HandleRunAudio(move, isRunning);

        // Jump logic
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && knockbackTimer <= 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.Play("JumpStart");
            isJumping = true;

            // NEW: Play jump sound once
            if (sfxAudioSource != null && jumpClip != null)
            {
                sfxAudioSource.PlayOneShot(jumpClip);
            }
        }

        ApplyGravity();

        // Example: Kill player with '-'
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

    // NEW: Handle footstep / running sound
    void HandleRunAudio(Vector3 move, bool isRunning)
    {
        // If we're moving (magnitude > 0) AND grounded
        bool isMoving = move.magnitude > 0.01f && isGrounded && !isJumping;

        // If using "runClip" for both walk/run, you'll play the same footsteps
        // Or you could add a separate "walkClip" if you want different sounds.
        if (isMoving)
        {
            // If not currently playing run sound, start it
            if (!isPlayingRunSound && sfxAudioSource != null && runClip != null)
            {
                sfxAudioSource.clip = runClip;
                sfxAudioSource.loop = true;
                sfxAudioSource.Play();
                isPlayingRunSound = true;
            }
        }
        else
        {
            // Stop the looped running if no longer moving
            if (isPlayingRunSound && sfxAudioSource != null)
            {
                sfxAudioSource.Stop();
                sfxAudioSource.clip = null;
                isPlayingRunSound = false;
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
        float damping = 1 - (knockbackTimer / knockbackDuration);
        Vector3 knockback = knockbackDirection * knockbackForce * damping * Time.deltaTime;
        controller.Move(knockback);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackDirection = direction.normalized;
        knockbackForce = force;
        knockbackTimer = knockbackDuration;

        // Reset vertical velocity for better knockback feel
        velocity.y = 0;

        // Trigger hit animation
        anim.SetTrigger("IsHit");
    }
}
