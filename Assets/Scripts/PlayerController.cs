using System;
using System.Collections;
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

    // Movement state
    private float coyoteTimeCounter;
    private bool jumpPressed;
    private bool jumpConsumed;
    private bool isStomping;
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
    private MovingPlatform currentPlatform;

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

        controls.Player.Jump.performed += ctx => jumpPressed = true;
        controls.Player.Jump.canceled  += ctx => { jumpPressed = false; jumpConsumed = false; };
    }

    private void OnEnable()  => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

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

        wasGrounded = isGrounded;

        // ── Coyote time ───────────────────────────────────────────────────────
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // ── Climbing ──────────────────────────────────────────────────────────
        climbCooldown -= Time.deltaTime;

        // Reset bottom-exit block when player presses up or leaves the ladder
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
                // Reached bottom of stairs — return to normal movement
                exitedClimbAtBottom = true;
                ExitClimb();
            }
            else
            {
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, moveInput.y * climbSpeed);

                // Jump off stairs
                if (jumpPressed && !jumpConsumed)
                {
                    ExitClimb();
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    anim.SetTrigger("jumpUp");
                    coyoteTimeCounter = 0f;
                    jumpConsumed = true;
                }
            }
        }

        // Still climbing after the checks above — lock animator and skip normal movement
        if (isClimbing)
        {
            anim.SetBool("isClimbing", true);
            anim.SetFloat("Speed", Mathf.Abs(moveInput.y));
            anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
            anim.SetBool("isGrounded", false);
            anim.speed = Mathf.Abs(moveInput.y) > 0.1f ? 1f : 0f;
            velocity = rb.linearVelocity.y;
            return;
        }

        anim.SetBool("isClimbing", false);

        // ── Normal movement ───────────────────────────────────────────────────
        float platformVelX = (currentPlatform != null && currentPlatform.IsActive)
            ? currentPlatform.Velocity.x : 0f;
        float targetSpeed = moveInput.x * moveSpeed + platformVelX;
        float accel  = isGrounded ? groundAcceleration : airAcceleration;
        float newX   = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.deltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        // ── Jump ──────────────────────────────────────────────────────────────
        if (jumpPressed && !jumpConsumed && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jumpUp");
            coyoteTimeCounter = 0f;
            jumpConsumed = true;
        }

        // ── Animator ──────────────────────────────────────────────────────────
        float animSpeed = Mathf.Abs(rb.linearVelocity.x);
        if (Mathf.Abs(moveInput.x) < 0.01f && isGrounded)
            animSpeed = 0f;

        anim.SetFloat("Speed", animSpeed);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        velocity = rb.linearVelocity.y;
    }

    private void FixedUpdate()
    {
        if (isClimbing) return; // velocity is set in Update while climbing
        if (lockMovementTimer > 0f) return; // preserve knockback velocity

        float platformVelX = (currentPlatform != null && currentPlatform.IsActive)
            ? currentPlatform.Velocity.x : 0f;
        float slopeBoost  = isGrounded && !jumpPressed ? 1.5f : 1f;
        float targetSpeed = moveInput.x * moveSpeed * slopeBoost + platformVelX;
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

    // ── Climbing helpers ──────────────────────────────────────────────────────

    private void EnterClimb()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        col.isTrigger = true;
        wasAirborneWhileClimbing = false;
    }

    private void ExitClimb()
    {
        isClimbing = false;
        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        anim.speed = 1f;
        col.isTrigger = false;
        climbCooldown = 0.25f;
    }

    // ── Trigger detection ─────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Climbable>(out _))
        {
            isOnLadder = true;
            Debug.Log("[PlayerController] isOnLadder = true");
            return;
        }

        if (other.CompareTag("Stomp"))
        {
            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                isStomping = true;
                enemy.TakeDamage(1);

                Collider2D enemyCollider = other.transform.parent.GetComponent<Collider2D>();
                if (enemyCollider != null)
                    StartCoroutine(IgnoreCollisionRoutine(enemyCollider));

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                anim.SetTrigger("jumpUp");
                coyoteTimeCounter = 0f;
                Invoke(nameof(ResetStomp), 0.1f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Climbable>(out _))
        {
            isOnLadder = false;
            if (isClimbing) ExitClimb();
        }
    }

    // ── Collision ────────────────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isStomping) return;

            // Landing on top of the enemy — let the stomp trigger handle it
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y > 0.5f) return;

            if (TryGetComponent<PlayerHealth>(out PlayerHealth ph))
                ph.TakeEnemyDamage(1);
        }

        TrySetPlatform(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) => TrySetPlatform(collision);

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlatform != null && collision.gameObject == currentPlatform.gameObject)
            currentPlatform = null;
    }

    private void TrySetPlatform(Collision2D collision)
    {
        if (currentPlatform != null) return;
        if (!collision.gameObject.TryGetComponent<MovingPlatform>(out MovingPlatform platform)) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                currentPlatform = platform;
                return;
            }
        }
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

            boxRb.linearVelocity = new Vector2(dir * boxPushSpeed, boxRb.linearVelocity.y);
            return;
        }
    }

    private void ResetStomp() => isStomping = false;

    private IEnumerator IgnoreCollisionRoutine(Collider2D enemyCollider)
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemyCollider, true);
        yield return new WaitForSeconds(0.2f);
        if (enemyCollider != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemyCollider, false);
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
