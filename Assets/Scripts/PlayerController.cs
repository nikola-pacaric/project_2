using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float airSpeed = 2.5f;
    public float jumpForce = 12f;
    public float groundAcceleration = 20f;
    public float airAcceleration = 5f;
    public float coyoteTime = 0.2f;

    public float coyoteTimeCounter;

    private Rigidbody2D rb;
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isStomping;

    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;
    [Header("Pushable Box")]
    [SerializeField] private float boxPushSpeed = 3f;
    [SerializeField] private float boxPushCastDistance = 0.12f;
    [SerializeField] private float minPushInput = 0.1f;
    public bool isGrounded;
    private bool wasGrounded;
    private float lockMovementTimer;

    private Animator anim;
    private Collider2D col;
    public float velocity;
    public float MoveInputX => moveInput.x;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpPressed = true;
        controls.Player.Jump.canceled += ctx => jumpPressed = false;
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    
    void Update()
    {
        if (lockMovementTimer > 0f)
        {
            lockMovementTimer -= Time.deltaTime;
            return; // skip movement and animation updates while locked
        }

        if (moveInput.x > 0.05f)
        {
            transform.localScale = new Vector3(1, 1, 1);   // facing right
        }
        else if (moveInput.x < -0.05f)
        {
            transform.localScale = new Vector3(-1, 1, 1);  // facing left
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Reset vertical velocity when landing
        if (isGrounded && !wasGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        wasGrounded = isGrounded;

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime; // reset when grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // count down when airborne
        }

        float targetSpeed = moveInput.x * moveSpeed;
        float accel = isGrounded ? groundAcceleration : airAcceleration;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.deltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        if (jumpPressed && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jumpUp");
            coyoteTimeCounter = 0f;
            jumpPressed = false;
        }

        float animSpeed = Mathf.Abs(rb.linearVelocity.x);
        if (Mathf.Abs(moveInput.x) < 0.01f && isGrounded)
        {
            animSpeed = 0f;
        }
        anim.SetFloat("Speed", animSpeed);
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        velocity = rb.linearVelocity.y;
    }

    void FixedUpdate()
    {
        float slopeBoost = isGrounded && !jumpPressed ? 1.5f : 1f;
        float targetSpeed = moveInput.x * moveSpeed * slopeBoost;
        float accel = isGrounded ? groundAcceleration : airAcceleration;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.deltaTime);
        Vector2 desiredVelocity = new Vector2(newX, rb.linearVelocity.y);

        if (isGrounded && !jumpPressed) // only slide when grounded AND not jumping
        {
            var slideMovement = new Rigidbody2D.SlideMovement
            {
                useSimulationMove = true
            };

            var results = rb.Slide(desiredVelocity, Time.fixedDeltaTime, slideMovement);
            

            // Animator uses the actual applied velocity
            anim.SetFloat("Speed", Mathf.Abs(results.remainingVelocity.x));
            anim.SetBool("isGrounded", true);
        }
        else
        {
            // Airborne movement: let physics handle it
            rb.linearVelocity = desiredVelocity;
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("isGrounded", false);
        }

        PushBox();
    }

    private void PushBox()
    {
        if (!isGrounded) return;
        if (col == null) return;

        float inputX = moveInput.x;
        if (Mathf.Abs(inputX) < minPushInput) return;

        float dir = Mathf.Sign(inputX);
        Bounds b = col.bounds;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            b.center,
            b.size * 0.95f,
            0f,
            new Vector2(dir, 0f),
            boxPushCastDistance
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stomp"))
        {
            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                isStomping = true;
                enemy.TakeDamage(1);

                Collider2D enemyCollider = other.transform.parent.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    StartCoroutine(IgnoreCollisionRoutine(enemyCollider));
                }

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                anim.SetTrigger("jumpUp");

                coyoteTimeCounter = 0f;

                Invoke(nameof(ResetStomp), 0.1f);
            }
        }
    }

    private void ResetStomp()
    {
        isStomping = false;
    }

    private IEnumerator IgnoreCollisionRoutine(Collider2D enemyCollider)
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemyCollider, true);
        yield return new WaitForSeconds(0.2f);
        if (enemyCollider != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemyCollider, false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isStomping) return;

            PlayerHealth ph = GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeEnemyDamage(1);
            }
        }
    }

    public void LockMovement(float duration) => lockMovementTimer = duration;

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }

}
