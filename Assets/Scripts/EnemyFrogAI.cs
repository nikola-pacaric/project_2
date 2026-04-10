using UnityEngine;
using System.Collections;

public class EnemyFrogAI : MonoBehaviour
{
    [Header("Jump Physics")]
    public float jumpHeight = 5f;
    public float jumpForward = 3f;
    public float timeBetweenJumps = 1.0f;

    [Header("Route Settings")]
    public float patrolDistance = 5f;
    public float tauntDuration = 1.5f;

    [Header("Detection")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Patrol Sensors")]
    [SerializeField] private float destGroundCheckDepth = 1.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private float leftX, rightX, targetX;
    private bool isGrounded;
    private bool isActing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        leftX = transform.position.x - patrolDistance;
        rightX = transform.position.x + patrolDistance;
        targetX = rightX;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("isJumping", !isGrounded);

        if (isGrounded && !isActing)
        {
            if (Mathf.Abs(transform.position.x - targetX) < 0.5f || !IsDestinationSafe())
                StartCoroutine(TauntSequence());
            else
                StartCoroutine(JumpSequence());
        }
    }

    // Returns false if there's a wall blocking the path or no ground at the destination.
    private bool IsDestinationSafe()
    {
        float dir = targetX > transform.position.x ? 1f : -1f;
        float dist = Mathf.Abs(targetX - transform.position.x);

        bool wallBlocking = Physics2D.Raycast(
            rb.position + Vector2.up * 0.2f,
            Vector2.right * dir,
            dist,
            groundLayer
        );

        Vector2 destCheck = new Vector2(targetX, transform.position.y + 0.1f);
        bool groundAtDest = Physics2D.Raycast(destCheck, Vector2.down, destGroundCheckDepth, groundLayer);

        return !wallBlocking && groundAtDest;
    }

    private IEnumerator JumpSequence()
    {
        isActing = true;

        float direction = (targetX > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * jumpForward, jumpHeight);
        anim.SetTrigger("frog_jumps");

        yield return new WaitForSeconds(0.1f);

        float timeout = 2f;
        float timer = 0f;

        while (!isGrounded && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        rb.gravityScale = 5f;

        yield return new WaitForSeconds(timeBetweenJumps);

        rb.gravityScale = 1f;

        isActing = false;
    }

    private IEnumerator TauntSequence()
    {
        isActing = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetBool("isTaunting", true);
        yield return new WaitForSeconds(tauntDuration);
        anim.SetBool("isTaunting", false);

        targetX = (targetX == rightX) ? leftX : rightX;
        sprite.flipX = (targetX == leftX);

        isActing = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (!Application.isPlaying) return;

        float dir = targetX > transform.position.x ? 1f : -1f;
        float dist = Mathf.Abs(targetX - transform.position.x);

        // Wall check ray (red)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.2f, Vector3.right * dir * dist);

        // Destination ground check (cyan)
        Gizmos.color = Color.cyan;
        Vector3 destCheck = new Vector3(targetX, transform.position.y + 0.1f, 0f);
        Gizmos.DrawRay(destCheck, Vector3.down * destGroundCheckDepth);
    }
}
