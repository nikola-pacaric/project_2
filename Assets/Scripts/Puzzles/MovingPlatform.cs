using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Moves between two world positions calculated from spawn position + offsets.
/// No child Transform references needed — set offsets in Inspector.
/// Place in Assets/Scripts/Puzzles/
/// Requires: Rigidbody2D (Kinematic) + BoxCollider2D (NOT trigger)
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    // ── Serialized Fields ─────────────────────────────────────────────────────

    [SerializeField] private Vector2 offsetA = Vector2.zero;
    [SerializeField] private Vector2 offsetB = new Vector2(4f, 0f);

    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private bool startActive = false;
    [SerializeField] private bool looping = true;

    [SerializeField] private SpriteRenderer platformSprite;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color activeColor = Color.white;

    public UnityEvent onReachedPointB;
    public UnityEvent onReachedPointA;

    // ── Private Fields ────────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private bool isActive = false;
    private bool movingToB = true;

    private Vector2 worldPointA;
    private Vector2 worldPointB;

    private readonly List<Collider2D> riders = new List<Collider2D>();

    // ── Properties ────────────────────────────────────────────────────────────

    public bool IsActive => isActive;

    // ── Awake / Start ─────────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (platformSprite == null)
            platformSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Bake world positions once at spawn — offsets are now fixed in world space
        worldPointA = (Vector2)transform.position + offsetA;
        worldPointB = (Vector2)transform.position + offsetB;

        UpdateVisual();

        if (startActive)
            Activate();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        UpdateVisual();
        StartCoroutine(MoveRoutine());
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        StopAllCoroutines();
        UpdateVisual();
    }

    // ── Private Methods ───────────────────────────────────────────────────────

    private IEnumerator MoveRoutine()
    {
        while (isActive)
        {
            Vector2 target = movingToB ? worldPointB : worldPointA;

            while (isActive && Vector2.Distance(rb.position, target) > 0.02f)
            {
                Vector2 newPos = Vector2.MoveTowards(
                    rb.position,
                    target,
                    speed * Time.fixedDeltaTime
                );

                CarryRiders(newPos - rb.position);
                rb.MovePosition(newPos);

                yield return new WaitForFixedUpdate();
            }

            CarryRiders(target - rb.position);
            rb.MovePosition(target);

            if (movingToB) onReachedPointB?.Invoke();
            else onReachedPointA?.Invoke();

            yield return new WaitForSeconds(waitTime);

            if (!looping) break;

            movingToB = !movingToB;
        }
    }

    private void CarryRiders(Vector2 delta)
    {
        if (delta == Vector2.zero) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 checkCenter = rb.position + col.offset + Vector2.up * (col.size.y * 0.5f + 0.05f);
        Vector2 checkSize = new Vector2(col.size.x * 0.9f, 0.1f);

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;

        riders.Clear();
        Physics2D.OverlapBox(checkCenter, checkSize, 0f, filter, riders);

        foreach (Collider2D rider in riders)
        {
            Rigidbody2D riderRb = rider.attachedRigidbody;
            if (riderRb == null || riderRb == rb) continue;

            riderRb.position += delta;
        }
    }

    private void UpdateVisual()
    {
        if (platformSprite != null)
            platformSprite.color = isActive ? activeColor : inactiveColor;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Vector2 origin = (Vector2)transform.position;
        Vector2 pointA = origin + offsetA;
        Vector2 pointB = origin + offsetB;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pointA, 0.15f);
        Gizmos.DrawSphere(pointB, 0.15f);
        Gizmos.DrawLine(pointA, pointB);
    }
}