using UnityEngine;
using System.Collections;

public class EnemyFrogAI : MonoBehaviour
{
    [Header("Jump Physics")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float jumpForward = 3f;
    [SerializeField] private float timeBetweenJumps = 1.0f;

    [Header("Route Settings")]
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float tauntDuration = 1.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

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
    private bool IsDestinationSafe() => IsTargetSafe(targetX);

    private bool IsTargetSafe(float target)
    {
        float dir = target > transform.position.x ? 1f : -1f;
        float dist = Mathf.Abs(target - transform.position.x);

        bool wallBlocking = Physics2D.Raycast(
            rb.position + Vector2.up * 0.2f,
            Vector2.right * dir,
            dist,
            groundLayer
        );

        Vector2 destCheck = new Vector2(target, transform.position.y + 0.1f);
        bool groundAtDest = Physics2D.Raycast(destCheck, Vector2.down, destGroundCheckDepth, groundLayer);

        return !wallBlocking && groundAtDest;
    }

    private IEnumerator JumpSequence()
    {
        isActing = true;

        float direction = (targetX > transform.position.x) ? 1 : -1;
        float remaining = Mathf.Abs(targetX - transform.position.x);
        float horizSpeed = Mathf.Min(jumpForward, remaining);
        rb.linearVelocity = new Vector2(direction * horizSpeed, jumpHeight);
        anim.SetTrigger("frog_jumps");
        AudioManager.Instance?.PlaySFXAt(SfxId.EnemyFrogJump, transform.position);

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
        AudioManager.Instance?.PlaySFXAt(SfxId.EnemyFrogTaunt, transform.position);
        yield return new WaitForSeconds(tauntDuration);
        anim.SetBool("isTaunting", false);

        float newTarget = (targetX == rightX) ? leftX : rightX;
        if (IsTargetSafe(newTarget))
        {
            targetX = newTarget;
            sprite.flipX = (targetX == leftX);
        }

        isActing = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Patrol range (yellow) — visible in edit mode
        Vector3 originPos = Application.isPlaying
            ? new Vector3((leftX + rightX) * 0.5f, transform.position.y, 0f)
            : transform.position;
        Vector3 leftEnd = new Vector3(originPos.x - patrolDistance, originPos.y, 0f);
        Vector3 rightEnd = new Vector3(originPos.x + patrolDistance, originPos.y, 0f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(leftEnd, rightEnd);
        Gizmos.DrawWireSphere(leftEnd, 0.15f);
        Gizmos.DrawWireSphere(rightEnd, 0.15f);

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
