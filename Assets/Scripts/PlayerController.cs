using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;

    [Header("Movement")]
    public float maxMoveSpeed = 6f;
    public float acceleration = 18f;
    public float deceleration = 12f;
    public float airControlMultiplier = 0.35f;

    [Header("Jump")]
    public float gravity = -22f;
    public float jumpHeight = 2f;
    public float jumpDirectionalBoost = 5f;

    [Header("Ground Check")]
    public float groundRadius = 0.3f;

    private InputSystem_Actions controls;
    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 currentHorizontalVelocity;
    private bool jumpPressed;
    private bool isGrounded;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpPressed = true;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        // ===== LEFT / RIGHT (ACCELERATION) =====
        float targetSideSpeed = moveInput.x * maxMoveSpeed;

        float control = isGrounded ? 1f : airControlMultiplier;
        float accelRate = Mathf.Abs(moveInput.x) > 0.01f ? acceleration : deceleration;

        float currentSideVelocity = currentHorizontalVelocity.x;

        currentSideVelocity = Mathf.MoveTowards(
            currentSideVelocity,
            targetSideSpeed,
            accelRate * control * Time.deltaTime
        );

        // ===== FORWARD / BACK (DIRECT) =====
        float forwardSpeed = moveInput.y * maxMoveSpeed;

        // ===== FINAL VELOCITY =====
        currentHorizontalVelocity = new Vector3(
            currentSideVelocity,
            0f,
            forwardSpeed
        );

        controller.Move(currentHorizontalVelocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            Vector3 jumpDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            if (jumpDirection.magnitude > 0.1f)
            {
                currentHorizontalVelocity += jumpDirection * jumpDirectionalBoost;
            }
        }

        jumpPressed = false;
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}