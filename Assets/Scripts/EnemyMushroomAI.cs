using System.Collections;
using UnityEngine;

public class EnemyMushroomAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;

    [Header("Patrol Sensors")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDist = 0.3f;
    [SerializeField] private float ledgeCheckForward = 0.4f;
    [SerializeField] private float ledgeCheckHeightOffset = 0.3f;
    [SerializeField] private float ledgeCheckDepth = 0.8f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;

    [Header("Attack")]
    [SerializeField] private GameObject gasCloudPrefab;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private float preAttackDuration = 0.8f;
    [SerializeField] private float attackWindUp = 0.3f;
    [SerializeField] private float postAttackCooldown = 3f;

    private enum State { Patrol, PreAttack, Attack, Dead }
    private State state = State.Patrol;

    private Rigidbody2D rb;
    private Animator anim;

    private float leftX, rightX;
    private float moveDir = 1f;
    private bool isActing = false;
    private float cooldownTimer = 0f;
    private float flipCooldown = 0f;
    private Transform playerTransform;

    private const float FLIP_COOLDOWN = 0.3f;

    private void Start()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        leftX = transform.position.x - patrolDistance;
        rightX = transform.position.x + patrolDistance;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (state == State.Dead) return;

        cooldownTimer -= Time.deltaTime;
        flipCooldown -= Time.deltaTime;

        if (state == State.Patrol)
            HandlePatrol();
    }

    private void HandlePatrol()
    {
        if (isActing) return;

        if (flipCooldown <= 0f && ShouldFlip())
        {
            moveDir = -moveDir;
            flipCooldown = FLIP_COOLDOWN;
        }

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
        anim.Play("mushroom_walk");
        transform.localScale = new Vector3(moveDir, 1f, 1f);

        if (playerTransform != null && cooldownTimer <= 0f)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= detectionRange)
                StartCoroutine(AttackSequence());
        }
    }

    private bool ShouldFlip()
    {
        // Wall directly ahead
        bool wallAhead = Physics2D.Raycast(
            rb.position - Vector2.up * 0.4f,
            Vector2.right * moveDir,
            wallCheckDist,
            groundLayer
        );

        // No ground at foot level ahead (ledge)
        Vector2 ledgeOrigin = new Vector2(
            rb.position.x + moveDir * ledgeCheckForward,
            rb.position.y - ledgeCheckHeightOffset
        );
        bool groundAhead = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeCheckDepth, groundLayer);

        // Hard patrol boundary fallback
        bool atBoundary = transform.position.x >= rightX || transform.position.x <= leftX;

        return wallAhead || !groundAhead || atBoundary;
    }

    private IEnumerator AttackSequence()
    {
        isActing = true;
        state = State.PreAttack;

        rb.linearVelocity = Vector2.zero;
        float dir = playerTransform.position.x >= transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(dir, 1f, 1f);

        // Pre-attack pause — breath-no-gas idle
        anim.Play("mushroom_idle");
        yield return new WaitForSeconds(preAttackDuration);

        // Wind-up then fire
        state = State.Attack;
        anim.Play("mushroom_attack");
        yield return new WaitForSeconds(attackWindUp);

        if (gasCloudPrefab != null && mouthPoint != null)
        {
            GameObject gas = Instantiate(gasCloudPrefab, mouthPoint.position, Quaternion.identity);

            // Prevent the gas from triggering on the mushroom that spawned it
            if (TryGetComponent<Collider2D>(out Collider2D myCol) &&
                gas.TryGetComponent<Collider2D>(out Collider2D gasCol))
                Physics2D.IgnoreCollision(myCol, gasCol);

            if (gas.TryGetComponent<GasCloud>(out GasCloud gc))
                gc.Launch(dir);
        }

        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.5f);

        cooldownTimer = postAttackCooldown;
        state = State.Patrol;
        isActing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Wall check ray (red)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - Vector3.up * 0.4f, Vector3.right * moveDir * wallCheckDist);

        // Ledge check ray (cyan)
        Gizmos.color = Color.cyan;
        Vector3 ledgeOrigin = transform.position + Vector3.right * moveDir * ledgeCheckForward
                                                  + Vector3.down * ledgeCheckHeightOffset;
        Gizmos.DrawRay(ledgeOrigin, Vector3.down * ledgeCheckDepth);
    }
}
