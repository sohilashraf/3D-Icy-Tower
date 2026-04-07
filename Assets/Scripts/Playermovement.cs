using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float airControl = 0.5f;
    [SerializeField] private float groundFriction = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8.5f;
    [SerializeField] private float jumpForwardBoost = 3f;
    [SerializeField] private float jumpBufferTime = 0.18f;
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Wall")]
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private float wallJumpUpForce = 9.5f;
    [SerializeField] private float wallJumpAwayForce = 8.5f;
    [SerializeField] private float wallSlideSpeed = 6f;
    [SerializeField] private float wallJumpLockTime = 0.14f;
    [SerializeField] private float minFallSpeedForWallSlide = -0.5f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.3f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private const float ReverseBoost = 1.15f;
    private const float MaxAirForwardSpeedMultiplier = 1.25f;
    private const float WallJumpCooldown = 0.2f;
    private const float WallDetachDuration = 0.18f;
    private const float WallJumpCarryMomentum = 0.65f;
    private const float WallSlideAcceleration = 40f;
    private const float GroundCheckPadding = 0.12f;
    private const float GroundCheckRadiusScale = 0.9f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private InputSystem_Actions actions;

    private Vector2 moveInput;
    private bool jumpHeld;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private Vector3 wallNormal;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float wallJumpCooldownTimer;
    private float wallDetachTimer;
    private float wallJumpInputLockTimer;

    private Vector3 lastWallJumpNormal;

    private int speedHash;
    private int groundedHash;

    private bool wasFalling;
    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public bool IsWallSliding => isWallSliding;
    public Vector3 WallNormal => wallNormal;
    public Vector3 Velocity => rb.linearVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        if (animator == null)
            animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;

        actions = new InputSystem_Actions();

        actions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        actions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        actions.Player.Jump.performed += _ =>
        {
            jumpHeld = true;
            jumpBufferTimer = jumpBufferTime;
        };

        actions.Player.Jump.canceled += _ => jumpHeld = false;

        speedHash = Animator.StringToHash("Speed");
        groundedHash = Animator.StringToHash("Grounded");
    }

    private void OnEnable() => actions.Enable();
    private void OnDisable() => actions.Disable();
    private void OnDestroy() => actions.Dispose();

    private void Update()
    {
        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        wallJumpCooldownTimer -= Time.deltaTime;
        wallDetachTimer -= Time.deltaTime;
        wallJumpInputLockTimer -= Time.deltaTime;

        CheckGround();
        CheckWall();
        UpdateWallSlideState();

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            lastWallJumpNormal = Vector3.zero;
        }

        TryJump();
        UpdateAnimator();

        bool isFalling = !isGrounded && rb.linearVelocity.y < -1f;

        if (isFalling && !wasFalling)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayFall();
        }

        wasFalling = isFalling;
    }

    private void FixedUpdate()
    {
        ApplySideMovement();
        ApplyForwardMovement();
        ApplyCustomGravity();
        ClampHorizontalSpeeds();
    }

    private void CheckGround()
    {
        isGrounded = Physics.SphereCast(
            GetCapsuleBottomHemisphereCenter(),
            capsule.radius * GroundCheckRadiusScale,
            Vector3.down,
            out _,
            GroundCheckPadding,
            groundLayer,
            QueryTriggerInteraction.Ignore);
    }

    private void CheckWall()
    {
        if (wallDetachTimer > 0f || wallJumpCooldownTimer > 0f)
        {
            isTouchingWall = false;
            wallNormal = Vector3.zero;
            return;
        }

        if (isGrounded && rb.linearVelocity.y <= 0f)
        {
            isTouchingWall = false;
            wallNormal = Vector3.zero;
            return;
        }

        isTouchingWall = false;
        wallNormal = Vector3.zero;

        Vector3 origin = transform.TransformPoint(capsule.center);
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;

        Vector3[] dirs =
        {
            right,
            -right,
            (right + forward * 0.35f).normalized,
            (-right + forward * 0.35f).normalized
        };

        float bestScore = -999f;

        foreach (Vector3 dir in dirs)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, wallCheckDistance, wallLayer, QueryTriggerInteraction.Ignore))
            {
                float score = Vector3.Dot(-hit.normal, dir);
                if (score > bestScore)
                {
                    bestScore = score;
                    isTouchingWall = true;
                    wallNormal = hit.normal;
                }
            }
        }
    }

    private void UpdateWallSlideState()
    {
        if (wallDetachTimer > 0f)
        {
            isWallSliding = false;
            return;
        }

        isWallSliding =
            isTouchingWall &&
            !isGrounded &&
            rb.linearVelocity.y < minFallSpeedForWallSlide;
    }

    private void TryJump()
    {
        if (jumpBufferTimer <= 0f)
            return;

        if (coyoteTimer > 0f)
        {
            PerformGroundJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            return;
        }

        if (isTouchingWall)
        {
            PerformWallJump();
            jumpBufferTimer = 0f;
        }
    }

    private void PerformGroundJump()
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.y < 0f)
            velocity.y = 0f;

        velocity.y = jumpForce;
        velocity += transform.forward * jumpForwardBoost;

        rb.linearVelocity = velocity;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayJump();
    }

    private void PerformWallJump()
    {
        lastWallJumpNormal = wallNormal;

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

        horizontal *= WallJumpCarryMomentum;
        horizontal += wallNormal * wallJumpAwayForce;
        horizontal += transform.forward * jumpForwardBoost;

        rb.linearVelocity = new Vector3(horizontal.x, wallJumpUpForce, horizontal.z);

        wallJumpCooldownTimer = WallJumpCooldown;
        wallDetachTimer = WallDetachDuration;
        wallJumpInputLockTimer = wallJumpLockTime;

        isTouchingWall = false;
        isWallSliding = false;
        wallNormal = Vector3.zero;
    }

    private void ApplySideMovement()
    {
        Vector3 sideAxis = transform.right;
        float sideInput = moveInput.x;

        Vector3 velocity = rb.linearVelocity;
        float currentSideSpeed = Vector3.Dot(velocity, sideAxis);

        if (Mathf.Abs(sideInput) > 0.01f)
        {
            float targetSpeed = sideInput * moveSpeed;
            float accel = isGrounded ? moveSpeed * 7.5f : moveSpeed * 7.5f * airControl;

            bool reversing = Mathf.Sign(targetSpeed) != Mathf.Sign(currentSideSpeed) && Mathf.Abs(currentSideSpeed) > 0.05f;
            if (reversing)
                accel *= ReverseBoost;

            float newSideSpeed = Mathf.MoveTowards(currentSideSpeed, targetSpeed, accel * Time.fixedDeltaTime);
            velocity += sideAxis * (newSideSpeed - currentSideSpeed);

            velocity = FilterWallReattachInput(velocity);
            velocity = RemoveIntoWallVelocity(velocity);

            rb.linearVelocity = velocity;
        }
        else if (isGrounded)
        {
            ApplyAxisFriction(sideAxis, groundFriction);
        }
    }

    private void ApplyForwardMovement()
    {
        Vector3 forwardAxis = transform.forward;
        float forwardInput = moveInput.y;

        Vector3 velocity = rb.linearVelocity;
        float currentForwardSpeed = Vector3.Dot(velocity, forwardAxis);

        if (Mathf.Abs(forwardInput) > 0.01f)
        {
            float targetSpeed = forwardInput * moveSpeed;
            float accel = isGrounded ? moveSpeed * 6.8f : moveSpeed * 6.8f * airControl;

            bool reversing = Mathf.Sign(targetSpeed) != Mathf.Sign(currentForwardSpeed) && Mathf.Abs(currentForwardSpeed) > 0.05f;
            if (reversing)
                accel *= ReverseBoost;

            float maxAllowed = isGrounded ? moveSpeed : moveSpeed * MaxAirForwardSpeedMultiplier;
            targetSpeed = Mathf.Clamp(targetSpeed, -maxAllowed, maxAllowed);

            float newForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, accel * Time.fixedDeltaTime);
            velocity += forwardAxis * (newForwardSpeed - currentForwardSpeed);

            velocity = FilterWallReattachInput(velocity);
            velocity = RemoveIntoWallVelocity(velocity);

            rb.linearVelocity = velocity;
        }
        else if (isGrounded)
        {
            ApplyAxisFriction(forwardAxis, groundFriction * 0.8f);
        }
    }

    private Vector3 FilterWallReattachInput(Vector3 velocity)
    {
        if (wallJumpInputLockTimer <= 0f || lastWallJumpNormal == Vector3.zero)
            return velocity;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float intoWall = Vector3.Dot(horizontal, -lastWallJumpNormal);

        if (intoWall > 0f)
            horizontal += lastWallJumpNormal * intoWall;

        return new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private Vector3 RemoveIntoWallVelocity(Vector3 velocity)
    {
        if (!isTouchingWall || wallNormal == Vector3.zero)
            return velocity;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float intoWall = Vector3.Dot(horizontal, -wallNormal);

        if (intoWall > 0f)
            horizontal += wallNormal * intoWall;

        return new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private void ApplyAxisFriction(Vector3 axis, float friction)
    {
        Vector3 velocity = rb.linearVelocity;
        float along = Vector3.Dot(velocity, axis);
        float newAlong = Mathf.MoveTowards(along, 0f, friction * Time.fixedDeltaTime);
        rb.linearVelocity = velocity + axis * (newAlong - along);
    }

    private void ClampHorizontalSpeeds()
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 sideAxis = transform.right;
        Vector3 forwardAxis = transform.forward;

        float sideSpeed = Vector3.Dot(velocity, sideAxis);
        float forwardSpeed = Vector3.Dot(velocity, forwardAxis);

        float clampedSide = Mathf.Clamp(sideSpeed, -moveSpeed, moveSpeed);
        float clampedForward = isGrounded
            ? Mathf.Clamp(forwardSpeed, -moveSpeed, moveSpeed)
            : Mathf.Clamp(forwardSpeed, -moveSpeed * MaxAirForwardSpeedMultiplier, moveSpeed * MaxAirForwardSpeedMultiplier);

        Vector3 horizontal = sideAxis * clampedSide + forwardAxis * clampedForward;
        rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private void ApplyCustomGravity()
    {
        if (isGrounded && rb.linearVelocity.y <= 0f)
            return;

        if (isWallSliding)
        {
            Vector3 velocity = rb.linearVelocity;

            velocity.y = Mathf.MoveTowards(
                velocity.y,
                -wallSlideSpeed,
                WallSlideAcceleration * Time.fixedDeltaTime
            );

            rb.linearVelocity = velocity;
            return;
        }

        float gravityMultiplier = 1f;

        if (rb.linearVelocity.y < 0f)
            gravityMultiplier = fallGravityMultiplier;
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
            gravityMultiplier = lowJumpGravityMultiplier;

        float extraGravity = Physics.gravity.y * (gravityMultiplier - 1f);
        rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
    }

    private Vector3 GetCapsuleBottomHemisphereCenter()
    {
        float offset = capsule.height * 0.5f - capsule.radius;
        Vector3 local = capsule.center + Vector3.down * offset;
        return transform.TransformPoint(local);
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // while in air, stop feeding running animation
        float speed = isGrounded ? horizontalVelocity.magnitude : 0f;

        animator.SetFloat(speedHash, speed);
        animator.SetBool(groundedHash, isGrounded);
    }
}