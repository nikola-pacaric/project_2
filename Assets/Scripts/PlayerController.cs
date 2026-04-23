using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundAcceleration = 20f;
    [SerializeField] private float airAcceleration = 5f;
    [SerializeField] private float coyoteTime = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 4f;

    [Header("Pushable Box")]
    [SerializeField] private float boxPushSpeed = 3f;
    [SerializeField] private float boxPushCastDistance = 0.12f;
    [SerializeField] private float minPushInput = 0.1f;

    [Header("Stomp")]
    [SerializeField] private Hitbox stompHitbox;
    [SerializeField] private float stompBounceForce = 12f;

    // Movement state
    private float coyoteTimeCounter;
    private bool jumpPressed;
    private bool jumpQueued;
    private bool jumpConsumed;
    public bool isGrounded;
    private bool wasGrounded;
    private float lockMovementTimer;
    private float facingLockTimer;

    // Climbing state
    private bool isOnLadder;
    private bool isClimbing;
    private float originalGravityScale;
    private bool wasAirborneWhileClimbing;
    private float climbCooldown;
    private bool exitedClimbAtBottom;

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    private PlayerControls controls;

    // Input
    private Vector2 moveInput;

    // Public accessors
    public float velocity;
    public float MoveInputX => moveInput.x;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled  += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => { jumpPressed = true; jumpQueued = true; };
        controls.Player.Jump.canceled  += ctx => { jumpPressed = false; jumpQueued = false; };
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        if (stompHitbox != null)
        {
            stompHitbox.OnHitLanded += HandleStompLanded;
            stompHitbox.SetActive(false);
        }
    }

    private void OnDisable()
    {
        controls.Player.Disable();
        if (stompHitbox != null) stompHitbox.OnHitLanded -= HandleStompLanded;
    }

    private void Start()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col  = GetComponent<Collider2D>();
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (lockMovementTimer > 0f)
        {
            lockMovementTimer -= Time.deltaTime;
            UpdateStompHitbox();
            return;
        }

        // ── Facing direction ──────────────────────────────────────────────────
        facingLockTimer -= Time.deltaTime;
        if (facingLockTimer <= 0f)
        {
            if (moveInput.x > 0.05f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput.x < -0.05f)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // ── Ground check ─────────────────────────────────────────────────────
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded && !wasGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        if (isGrounded && rb.linearVelocity.y <= 0.1f)
            jumpConsumed = false;

        wasGrounded = isGrounded;

        // ── Coyote time ───────────────────────────────────────────────────────
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // ── Climbing ──────────────────────────────────────────────────────────
        climbCooldown -= Time.deltaTime;

        if (exitedClimbAtBottom && (!isOnLadder || moveInput.y > 0.1f))
            exitedClimbAtBottom = false;

        if (isOnLadder && !isClimbing && climbCooldown <= 0f && Mathf.Abs(moveInput.y) > 0.1f && !exitedClimbAtBottom)
            EnterClimb();

        if (isClimbing)
        {
            if (!isGrounded)
                wasAirborneWhileClimbing = true;

            if (!isOnLadder)
            {
                ExitClimb();
            }
            else if (isGrounded && moveInput.y < 0.1f && wasAirborneWhileClimbing)
            {
                exitedClimbAtBottom = true;
                ExitClimb();
            }
            else
            {
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, moveInput.y * climbSpeed);

                if (jumpQueued && !jumpConsumed)
                {
                    ExitClimb();
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    anim.SetTrigger("jumpUp");
                    AudioManager.Instance?.PlaySFX(SfxId.Jump);
                    coyoteTimeCounter = 0f;
                    jumpConsumed = true;
                    jumpQueued = false;
                }
            }
        }

        if (isClimbing)
        {
            anim.SetBool("isClimbing", true);
            anim.SetFloat("Speed", Mathf.Abs(moveInput.y));
            anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
            anim.SetBool("isGrounded", false);
            anim.speed = Mathf.Abs(moveInput.y) > 0.1f ? 1f : 0f;
            velocity = rb.linearVelocity.y;
            UpdateStompHitbox();
            return;
        }

        anim.SetBool("isClimbing", false);

        // ── Normal movement ───────────────────────────────────────────────────
        float targetSpeed = moveInput.x * moveSpeed;
        float accel  = isGrounded ? groundAcceleration : airAcceleration;
        float newX   = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.deltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        // ── Jump ──────────────────────────────────────────────────────────────
        if (jumpQueued && !jumpConsumed && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jumpUp");
            AudioManager.Instance?.PlaySFX(SfxId.Jump);
            coyoteTimeCounter = 0f;
            jumpConsumed = true;
            jumpQueued = false;
        }

        // ── Animator ──────────────────────────────────────────────────────────
        float animSpeed = Mathf.Abs(rb.linearVelocity.x);
        if (Mathf.Abs(moveInput.x) < 0.01f && isGrounded)
            animSpeed = 0f;

        anim.SetFloat("Speed", animSpeed);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        velocity = rb.linearVelocity.y;

        UpdateStompHitbox();
    }

    private void FixedUpdate()
    {
        if (isClimbing) return;
        if (lockMovementTimer > 0f) return;

        float slopeBoost  = isGrounded && !jumpPressed ? 1.5f : 1f;
        float targetSpeed = moveInput.x * moveSpeed * slopeBoost;
        float accel  = isGrounded ? groundAcceleration : airAcceleration;
        float newX   = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.deltaTime);
        Vector2 desiredVelocity = new Vector2(newX, rb.linearVelocity.y);

        if (isGrounded && !jumpPressed)
        {
            var slideMovement = new Rigidbody2D.SlideMovement { useSimulationMove = true };
            var results = rb.Slide(desiredVelocity, Time.fixedDeltaTime, slideMovement);
            anim.SetFloat("Speed", Mathf.Abs(results.remainingVelocity.x));
            anim.SetBool("isGrounded", true);
        }
        else
        {
            rb.linearVelocity = desiredVelocity;
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("isGrounded", false);
        }

        PushBox();
    }

    // ── Stomp ─────────────────────────────────────────────────────────────────

    private void UpdateStompHitbox()
    {
        if (stompHitbox == null) return;
        // Active whenever falling in-air. Hurtbox placement on stompable enemies
        // decides whether the stomp lands; shape, not code, gates stompability.
        bool falling = !isGrounded && rb != null && rb.linearVelocity.y < 0f;
        stompHitbox.SetActive(falling);
    }

    private void HandleStompLanded(Hurtbox target)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
        anim.SetTrigger("jumpUp");
        AudioManager.Instance?.PlaySFX(SfxId.Stomp);
        coyoteTimeCounter = 0f;
    }

    // ── Climbing helpers ──────────────────────────────────────────────────────

    private void EnterClimb()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        if (col != null) col.isTrigger = true;
        wasAirborneWhileClimbing = false;
        jumpConsumed = false;
    }

    private void ExitClimb()
    {
        isClimbing = false;
        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        anim.speed = 1f;
        if (col != null) col.isTrigger = false;
        climbCooldown = 0.25f;
    }

    // ── Trigger detection ─────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Climbable>(out _))
            isOnLadder = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<Climbable>(out _)) return;

        // Child colliders (stomp hitbox) toggle on/off every frame and bubble
        // their trigger events up to this Rigidbody2D. Only treat this as a
        // real ladder exit if the main body collider is no longer touching.
        if (col != null && col.IsTouching(other)) return;

        isOnLadder = false;
        if (isClimbing) ExitClimb();
    }

    // ── Misc helpers ─────────────────────────────────────────────────────────

    private void PushBox()
    {
        if (!isGrounded) return;
        if (col == null) return;

        float inputX = moveInput.x;
        if (Mathf.Abs(inputX) < minPushInput) return;

        float dir = Mathf.Sign(inputX);
        Bounds b  = col.bounds;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            b.center, b.size * 0.95f, 0f,
            new Vector2(dir, 0f), boxPushCastDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == col) continue;
            if (!hit.collider.CompareTag("PushableBox")) continue;

            Rigidbody2D boxRb = hit.rigidbody ?? hit.collider.attachedRigidbody;
            if (boxRb == null) continue;
            if (!boxRb.TryGetComponent<PushableBox>(out _)) continue;

            boxRb.linearVelocity = new Vector2(dir * boxPushSpeed, boxRb.linearVelocity.y);
            return;
        }
    }

    public void LockMovement(float duration) => lockMovementTimer = duration;
    public void LockFacing(float duration) => facingLockTimer = duration;

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
