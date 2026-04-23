using System.Collections;
using UnityEngine;

public class EnemyEagleAI : MonoBehaviour
{
    [Header("Hover Patrol")]
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;

    [Header("Detection")]
    [SerializeField] private float detectionRangeX = 5f;
    [SerializeField] private float detectionRangeY = 6f;

    [Header("Attack")]
    [SerializeField] private float spottedPause = 0.5f;
    [SerializeField] private float diveSpeed = 10f;
    [SerializeField] private float diveOvershoot = 1.5f;
    [SerializeField] private float riseSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float ceilingCheckDist = 1f;
    [SerializeField] private float ceilingCheckRadius = 0.25f;
    [SerializeField] private float stuckTeleportTime = 8f;

    [Header("Combat")]
    [Tooltip("Enabled only during Dive so player can't be damaged by a hovering eagle, " +
             "and can counter-stomp the dive.")]
    [SerializeField] private Hitbox contactHitbox;

    private enum State { Hover, Spotted, Dive, Rise, Dead }
    private State state = State.Hover;

    private Rigidbody2D rb;
    private Animator anim;
    private bool hitGroundDuringDive = false;

    private float leftX, rightX;
    private float moveDir = 1f;
    private float homeY;
    private Transform playerTransform;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;

        homeY = transform.position.y;
        leftX = transform.position.x - patrolDistance;
        rightX = transform.position.x + patrolDistance;

        if (contactHitbox != null) contactHitbox.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (state == State.Dead) return;
        if (state == State.Hover) HandleHover();
    }

    private void HandleHover()
    {
        rb.linearVelocity = new Vector2(moveDir * hoverSpeed, 0f);
        transform.localScale = new Vector3(-moveDir, 1f, 1f);

        if (transform.position.x >= rightX) moveDir = -1f;
        else if (transform.position.x <= leftX) moveDir = 1f;

        if (playerTransform == null) return;

        float dx = Mathf.Abs(playerTransform.position.x - transform.position.x);
        float dy = transform.position.y - playerTransform.position.y;

        if (dx <= detectionRangeX && dy > 0f && dy <= detectionRangeY)
            StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        // ── Spotted: telegraph pause ─────────────────────────────────────────
        state = State.Spotted;
        rb.linearVelocity = Vector2.zero;

        // Snap to face the player
        float faceDir = playerTransform.position.x >= transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(-faceDir, 1f, 1f);
        anim.SetBool("isDiving", true);
        AudioManager.Instance?.PlaySFXAt(SfxId.EnemyEagleAttack, transform.position);

        yield return new WaitForSeconds(spottedPause);

        // ── Dive: lock direction toward player's position right now ──────────
        state = State.Dive;
        if (contactHitbox != null) contactHitbox.SetActive(true);

        Vector2 diveDir = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 diveStart = rb.position;
        float diveDistance = Vector2.Distance(rb.position, (Vector2)playerTransform.position) + diveOvershoot;

        hitGroundDuringDive = false;
        while (Vector2.Distance(rb.position, diveStart) < diveDistance && !hitGroundDuringDive)
        {
            rb.linearVelocity = diveDir * diveSpeed;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        hitGroundDuringDive = false;

        // Switch to Rise now so OnCollisionEnter2D stops interfering
        state = State.Rise;
        if (contactHitbox != null) contactHitbox.SetActive(false);
        anim.SetBool("isDiving", false);

        float riseXDir = Mathf.Sign(diveDir.x);
        transform.localScale = new Vector3(-riseXDir, 1f, 1f);

        // ── Rise with ceiling navigation ─────────────────────────────────────
        float stuckTimer = 0f;
        while (transform.position.y < homeY)
        {
            bool ceilingAbove = Physics2D.CircleCast(rb.position, ceilingCheckRadius, Vector2.up, ceilingCheckDist, groundLayer);

            if (ceilingAbove) stuckTimer += Time.deltaTime;
            else stuckTimer = 0f;

            if (stuckTimer >= stuckTeleportTime)
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = new Vector3(transform.position.x, homeY, transform.position.z);
                break;
            }

            if (ceilingAbove)
            {
                // Platform above — move sideways, wait for a physics step so
                // rb.position actually updates before we check movement
                Vector2 prevPos = rb.position;
                rb.linearVelocity = new Vector2(riseXDir * hoverSpeed, 0f);
                yield return new WaitForFixedUpdate();

                // X didn't move after a real physics step — wall blocking, flip
                if (Mathf.Abs(rb.position.x - prevPos.x) < 0.02f)
                {
                    riseXDir = -riseXDir;
                    transform.localScale = new Vector3(-riseXDir, 1f, 1f);
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(riseXDir * hoverSpeed, riseSpeed);
                yield return new WaitForFixedUpdate();
            }
        }

        moveDir = -riseXDir;
        leftX = transform.position.x - patrolDistance;
        rightX = transform.position.x + patrolDistance;

        state = State.Hover;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) == 0) return;

        ContactPoint2D contact = collision.GetContact(0);

        // Feet hit ground during dive — stop dive
        if (state == State.Dive && contact.normal.y > 0.5f)
        {
            hitGroundDuringDive = true;
            rb.linearVelocity = Vector2.zero;
        }

    }

    private void OnDrawGizmosSelected()
    {
        // Detection box
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + Vector3.down * detectionRangeY * 0.5f,
            new Vector3(detectionRangeX * 2f, detectionRangeY, 0f)
        );

        // Dive overshoot ring
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, diveOvershoot);

        // Ceiling circle cast — origin sphere + swept sphere at tip
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, ceilingCheckRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * ceilingCheckDist, ceilingCheckRadius);
        Gizmos.DrawLine(transform.position + Vector3.left * ceilingCheckRadius,
                        transform.position + Vector3.up * ceilingCheckDist + Vector3.left * ceilingCheckRadius);
        Gizmos.DrawLine(transform.position + Vector3.right * ceilingCheckRadius,
                        transform.position + Vector3.up * ceilingCheckDist + Vector3.right * ceilingCheckRadius);
    }
}
