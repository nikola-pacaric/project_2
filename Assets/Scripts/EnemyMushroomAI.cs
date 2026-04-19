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
    [SerializeField] private float firstAttackDelay = 0.4f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackWindUp = 0.3f;
    [SerializeField] private float attackFinish = 0.3f;

    private Rigidbody2D rb;
    private Animator anim;

    private float leftX, rightX;
    private float moveDir = 1f;
    private float flipCooldown = 0f;
    private bool isAttacking = false;
    private Transform playerTransform;

    private const float FLIP_COOLDOWN = 0.3f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        leftX = transform.position.x - patrolDistance;
        rightX = transform.position.x + patrolDistance;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (!isAttacking)
            HandlePatrol();
    }

    private void HandlePatrol()
    {
        flipCooldown -= Time.deltaTime;

        if (flipCooldown <= 0f && ShouldFlip())
        {
            moveDir = -moveDir;
            flipCooldown = FLIP_COOLDOWN;
        }

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(moveDir, 1f, 1f);

        if (playerTransform != null &&
            Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
            StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        anim.SetBool("isShooting", true);

        bool firstShot = true;
        do
        {
            rb.linearVelocity = Vector2.zero;

            // Track player facing during cooldown
            float wait = firstShot ? firstAttackDelay : attackCooldown;
            float timer = 0f;
            while (timer < wait)
            {
                timer += Time.deltaTime;
                float facing = playerTransform.position.x >= transform.position.x ? 1f : -1f;
                transform.localScale = new Vector3(facing, 1f, 1f);
                yield return null;
            }
            firstShot = false;

            // Lock direction at moment of firing
            float dir = playerTransform.position.x >= transform.position.x ? 1f : -1f;
            transform.localScale = new Vector3(dir, 1f, 1f);

            // Trigger attack animation, fire gas at wind-up frame
            anim.SetTrigger("shoot");
            yield return new WaitForSeconds(attackWindUp);
            FireGas(dir);

            // Wait for attack clip to finish
            yield return new WaitForSeconds(attackFinish);
        }
        while (playerTransform != null &&
               Vector2.Distance(transform.position, playerTransform.position) <= detectionRange);

        anim.SetBool("isShooting", false);
        isAttacking = false;
    }

    private void FireGas(float dir)
    {
        if (gasCloudPrefab == null || mouthPoint == null) return;

        GameObject gas = Instantiate(gasCloudPrefab, mouthPoint.position, Quaternion.identity);
        AudioManager.Instance?.PlaySFXAt(SfxId.EnemyMushroomGas, transform.position);

        if (TryGetComponent<Collider2D>(out Collider2D myCol) &&
            gas.TryGetComponent<Collider2D>(out Collider2D gasCol))
            Physics2D.IgnoreCollision(myCol, gasCol);

        if (gas.TryGetComponent<GasCloud>(out GasCloud gc))
            gc.Launch(dir);
    }

    private bool ShouldFlip()
    {
        bool wallAhead = Physics2D.Raycast(
            rb.position - Vector2.up * 0.4f,
            Vector2.right * moveDir,
            wallCheckDist,
            groundLayer
        );

        Vector2 ledgeOrigin = new Vector2(
            rb.position.x + moveDir * ledgeCheckForward,
            rb.position.y - ledgeCheckHeightOffset
        );
        bool groundAhead = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeCheckDepth, groundLayer);

        bool atBoundary = transform.position.x >= rightX || transform.position.x <= leftX;

        return wallAhead || !groundAhead || atBoundary;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - Vector3.up * 0.4f, Vector3.right * moveDir * wallCheckDist);

        Gizmos.color = Color.cyan;
        Vector3 ledgeOrigin = transform.position + Vector3.right * moveDir * ledgeCheckForward
                                                  + Vector3.down * ledgeCheckHeightOffset;
        Gizmos.DrawRay(ledgeOrigin, Vector3.down * ledgeCheckDepth);
    }
}
