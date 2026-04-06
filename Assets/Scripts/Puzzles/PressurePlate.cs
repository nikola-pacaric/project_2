using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    // ── Serialized Fields ─────────────────────────────────────────────────────
    [SerializeField] private bool requiresBox = false;
    [SerializeField] private int requiredCount = 1;

    [Header("Press Animation")]
    [SerializeField] private Transform pressedVisual;
    [SerializeField] private float pressDepth = 0.08f;
    [SerializeField] private float pressSpeed = 20f;

    [SerializeField] private SpriteRenderer plateSprite;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = new Color(0.3f, 1f, 0.3f);

    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    // ── Private Fields ────────────────────────────────────────────────────────

    private int activatorCount = 0;
    private bool isActive = false;

    private Vector3 visualRestPosition;
    private Vector3 visualPressedPosition;
    private Vector3 visualTargetPosition;

    // ── Properties ────────────────────────────────────────────────────────────

    public bool IsActive => isActive;

    // ── Awake / Start ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (plateSprite == null && pressedVisual != null)
            plateSprite = pressedVisual.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (pressedVisual != null)
        {
            visualRestPosition = pressedVisual.localPosition;
            visualPressedPosition = visualRestPosition + Vector3.down * pressDepth;
            visualTargetPosition = visualRestPosition;
        }

        UpdateColor();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Reset activator count each frame — OnCollisionStay re-adds valid contacts
        activatorCount = 0;
    }

    // ── Collision Callbacks ───────────────────────────────────────────────────

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsValidActivator(collision.collider)) return;
        if (!IsPressedFromAbove(collision)) return;

        activatorCount++;
        EvaluateState();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsValidActivator(collision.collider)) return;

        // Force re-evaluation next frame with count at 0
        EvaluateState();
    }

    // ── FixedUpdate ───────────────────────────────────────────────────────────

    private void FixedUpdate()
    {
        // Smoothly lerp the visual to its target position
        if (pressedVisual == null) return;

        pressedVisual.localPosition = Vector3.Lerp(
            pressedVisual.localPosition,
            visualTargetPosition,
            pressSpeed * Time.fixedDeltaTime
        );
    }

    // ── Private Methods ───────────────────────────────────────────────────────

    private bool IsValidActivator(Collider2D other)
    {
        if (requiresBox)
            return other.CompareTag("PushableBox");

        return other.CompareTag("Player") || other.CompareTag("PushableBox");
    }

    private bool IsPressedFromAbove(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Normal points FROM plate TO the object above
            // If Y component is negative, contact is coming from above
            if (contact.normal.y < -0.5f)
                return true;
        }
        return false;
    }

    private void EvaluateState()
    {
        bool shouldBeActive = activatorCount >= requiredCount;

        if (shouldBeActive && !isActive)
        {
            isActive = true;
            visualTargetPosition = visualPressedPosition;
            UpdateColor();
            onActivated?.Invoke();
        }
        else if (!shouldBeActive && isActive)
        {
            isActive = false;
            visualTargetPosition = visualRestPosition;
            UpdateColor();
            onDeactivated?.Invoke();
        }
    }

    private void UpdateColor()
    {
        if (plateSprite != null)
            plateSprite.color = isActive ? activeColor : inactiveColor;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive
            ? new Color(0.3f, 1f, 0.3f, 0.4f)
            : new Color(1f, 1f, 0f, 0.3f);

        if (TryGetComponent<BoxCollider2D>(out var col))
            Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);
    }
}