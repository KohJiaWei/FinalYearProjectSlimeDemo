using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerBody;  // Reference to the player’s body (not the camera)

    public float walkSpeed = 5.0f;
    public float runSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    private Vector3 velocity;
    private bool isGrounded;
    public Animator anim;

    void Start()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        // Get movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Move relative to the player's forward direction, NOT the camera
        Vector3 move = playerBody.right * x + playerBody.forward * z;
        move.Normalize();  // Prevent diagonal movement from being faster

        // Determine speed
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Update animation speed
        float moveSpeed = move.magnitude;
        if (anim != null)
        {
            anim.SetFloat("Speed", moveSpeed);
        }

        // Jumping logic
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
