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
    private enum DeactivateBehavior { Freeze, SnapToA, SlideToA }
    private enum MovementAxis { Horizontal, Vertical }

    // ── Serialized Fields ─────────────────────────────────────────────────────

    [SerializeField] private MovementAxis axis = MovementAxis.Horizontal;
    [SerializeField] private Vector2 offsetA = Vector2.zero;
    [SerializeField] private Vector2 offsetB = new Vector2(4f, 0f);

    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private bool startActive = false;
    [SerializeField] private bool looping = true;
    [SerializeField] private DeactivateBehavior onDeactivate = DeactivateBehavior.Freeze;

    [SerializeField] private SpriteRenderer platformSprite;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color activeColor = Color.white;

    public UnityEvent onReachedPointB;
    public UnityEvent onReachedPointA;

    // ── Private Fields ────────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private bool isActive = false;
    private bool _isSliding = false;
    private bool movingToB = true;
    private float waitTimer = 0f;
    private Coroutine _returnCoroutine;

    private Vector2 worldPointA;
    private Vector2 worldPointB;

    private readonly List<Collider2D> riders = new List<Collider2D>();
    private readonly List<Rigidbody2D> pass1Rbs = new List<Rigidbody2D>();
    private readonly List<Collider2D> secondaryRiders = new List<Collider2D>();
    private ContactFilter2D carryFilter;

    // ── Properties ────────────────────────────────────────────────────────────

    public bool IsActive => isActive || _isSliding;
    public Vector2 Velocity { get; private set; }

    // ── Awake / Start ─────────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (platformSprite == null)
            platformSprite = GetComponent<SpriteRenderer>();

        carryFilter = new ContactFilter2D();
        carryFilter.useTriggers = false;
    }

    private void Start()
    {
        // Bake world positions once at spawn — axis determines which component is used
        Vector2 origin = (Vector2)transform.position;
        if (axis == MovementAxis.Horizontal)
        {
            worldPointA = origin + new Vector2(offsetA.x, 0f);
            worldPointB = origin + new Vector2(offsetB.x, 0f);
        }
        else
        {
            worldPointA = origin + new Vector2(0f, offsetA.y);
            worldPointB = origin + new Vector2(0f, offsetB.y);
        }

        UpdateVisual();

        if (startActive)
            Activate();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Activate()
    {
        if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
        if (isActive) return;
        isActive = true;
        movingToB = true;
        UpdateVisual();
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        UpdateVisual();

        switch (onDeactivate)
        {
            case DeactivateBehavior.SnapToA:
                rb.MovePosition(worldPointA);
                movingToB = true;
                break;
            case DeactivateBehavior.SlideToA:
                _returnCoroutine = StartCoroutine(SlideToA());
                break;
        }
    }

    // ── FixedUpdate ───────────────────────────────────────────────────────────

    private void FixedUpdate()
    {
        Velocity = Vector2.zero;

        if (!isActive) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 target = movingToB ? worldPointB : worldPointA;

        if (Vector2.Distance(rb.position, target) <= 0.02f)
        {
            CarryRiders(target - rb.position);
            rb.MovePosition(target);

            if (movingToB) onReachedPointB?.Invoke();
            else onReachedPointA?.Invoke();

            if (!looping) { Deactivate(); return; }

            movingToB = !movingToB;
            waitTimer = waitTime;
            return;
        }

        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        Vector2 delta = newPos - rb.position;
        Velocity = delta / Time.fixedDeltaTime;
        CarryRiders(delta);
        rb.MovePosition(newPos);
    }

    // ── Private Methods ───────────────────────────────────────────────────────

    private void CarryRiders(Vector2 delta)
    {
        if (delta == Vector2.zero) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();

        // Pass 1 — objects directly on the platform surface.
        pass1Rbs.Clear();
        riders.Clear();
        CheckAbove(rb.position + col.offset, col.size, riders);

        foreach (Collider2D rider in riders)
        {
            Rigidbody2D riderRb = rider.attachedRigidbody;
            if (riderRb == null || riderRb == rb) continue;

            if (TeleportRider(riderRb, delta))
                pass1Rbs.Add(riderRb);
        }

        // Pass 2 — objects on top of pass-1 riders (e.g. player standing on a
        // PressurePlate that is itself sitting on the platform).
        foreach (Rigidbody2D p1Rb in pass1Rbs)
        {
            if (!p1Rb.TryGetComponent<BoxCollider2D>(out BoxCollider2D p1Col)) continue;

            secondaryRiders.Clear();
            CheckAbove(p1Rb.position + p1Col.offset, p1Col.size, secondaryRiders);

            foreach (Collider2D rider in secondaryRiders)
            {
                Rigidbody2D riderRb = rider.attachedRigidbody;
                if (riderRb == null || riderRb == rb || pass1Rbs.Contains(riderRb)) continue;
                TeleportRider(riderRb, delta);
            }
        }
    }

    // Returns true if the rider was actually moved (so pass-2 can check above it).
    private bool TeleportRider(Rigidbody2D riderRb, Vector2 delta)
    {
        float xCarry = delta.x;
        // Dynamic riders: only carry upward — gravity handles downward naturally.
        // Kinematic riders (e.g. PressurePlate): carry both ways — no gravity to pull them down.
        float yCarry = riderRb.bodyType == RigidbodyType2D.Kinematic
            ? delta.y
            : (delta.y > 0f ? delta.y : 0f);

        if (xCarry == 0f && yCarry == 0f) return false;
        riderRb.position += new Vector2(xCarry, yCarry);
        return true;
    }

    private void CheckAbove(Vector2 center, Vector2 size, List<Collider2D> results)
    {
        Vector2 checkCenter = center + Vector2.up * (size.y * 0.5f + 0.05f);
        Vector2 checkSize   = new Vector2(size.x * 0.9f, 0.1f);
        Physics2D.OverlapBox(checkCenter, checkSize, 0f, carryFilter, results);
    }

    private IEnumerator SlideToA()
    {
        _isSliding = true;
        while (Vector2.Distance(rb.position, worldPointA) > 0.02f)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, worldPointA, speed * Time.fixedDeltaTime);
            Vector2 delta = newPos - rb.position;
            Velocity = delta / Time.fixedDeltaTime;
            CarryRiders(delta);
            rb.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(worldPointA);
        Velocity = Vector2.zero;
        movingToB = true;
        _isSliding = false;
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
        Vector2 pointA = axis == MovementAxis.Horizontal
            ? origin + new Vector2(offsetA.x, 0f)
            : origin + new Vector2(0f, offsetA.y);
        Vector2 pointB = axis == MovementAxis.Horizontal
            ? origin + new Vector2(offsetB.x, 0f)
            : origin + new Vector2(0f, offsetB.y);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pointA, 0.15f);
        Gizmos.DrawSphere(pointB, 0.15f);
        Gizmos.DrawLine(pointA, pointB);
    }
}