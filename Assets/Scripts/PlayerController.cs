using UnityEngine;

// All Farmhollow game scripts live in this namespace so their type
// names never collide with Unity package examples or third-party code.
namespace Farmhollow
{
    // ============================================================
    //  PlayerController
    //  Moves the player character with WASD / arrow keys.
    //  Belongs on the player character (which has a CharacterController).
    // ============================================================
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
    [Header("Movement")]
    public float moveSpeed = 5f;        // How fast the character walks
    public float rotationSpeed = 12f;   // How fast it turns to face the movement direction
    public float gravity = -20f;        // Gravity (pulls the character downward)

    [Header("Animation")]
    public Animator animator;           // the character model's Animator (auto-found if left empty)

    private CharacterController controller;
    private float verticalVelocity;     // current falling speed
    private static readonly int SpeedHash = Animator.StringToHash("Speed"); // animator parameter

    void Start()
    {
        // Grab the CharacterController component on the same object
        controller = GetComponent<CharacterController>();

        // If not assigned in the Inspector, find the Animator on the visual model (a child)
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // --- 1. Read input ---
        float h = Input.GetAxisRaw("Horizontal"); // A/D or left/right arrow
        float v = Input.GetAxisRaw("Vertical");   // W/S or up/down arrow

        // Movement direction in the world (X = sideways, Z = forward/back)
        Vector3 input = new Vector3(h, 0f, v).normalized;

        // --- 2. Apply gravity ---
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // press lightly into the ground (more stable "grounded")
        verticalVelocity += gravity * Time.deltaTime;

        // --- 3. Perform the movement ---
        Vector3 move = input * moveSpeed;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // --- 4. Turn the character toward the movement direction ---
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(input.x, 0f, input.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- 5. Tell the animator how fast we move (0 = idle, 1 = walking) ---
        // The 0.1s damping makes the blend between idle and walk smooth.
        if (animator != null)
            animator.SetFloat(SpeedHash, input.magnitude, 0.1f, Time.deltaTime);
    }
    }
}
