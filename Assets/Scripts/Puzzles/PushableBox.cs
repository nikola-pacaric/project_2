using UnityEngine;

public class PushableBox : MonoBehaviour
{
    [SerializeField] private float pushSpeed = 3.0f;
    [SerializeField] private float drag = 8f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private SpriteRenderer boxSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pushedColor = new Color(1.0f, 0.85f, 0.5f);

    [SerializeField] private float fallRespawnDistance = 5f;
    [SerializeField] private Transform respawnPoint;

    private Rigidbody2D rb;
    private Vector2 initialPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.linearDamping = drag;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        if (boxSprite == null) boxSprite = GetComponent<SpriteRenderer>();

        initialPosition = respawnPoint != null ? (Vector2)respawnPoint.position : (Vector2)transform.position;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < initialPosition.y - fallRespawnDistance)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = initialPosition;
        UpdateVisual(false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();

        if (pc == null || !pc.isGrounded) return;
        if (!TryGetPushDirection(collision, out float pushDir)) return;

        float inputX = pc.MoveInputX;
        if (Mathf.Abs(inputX) < 0.1f) return;
        if (Mathf.Sign(inputX) != Mathf.Sign(pushDir)) return;

        rb.linearVelocity = new Vector2(pushDir * pushSpeed, rb.linearVelocity.y);

        UpdateVisual(true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        UpdateVisual(false);
    }

    private void UpdateVisual(bool pushed)
    {
        if (boxSprite != null)
        {
            boxSprite.color = pushed ? pushedColor : normalColor;
        }
    }

    private bool TryGetPushDirection(Collision2D collision, out float pushDir)
    {
        pushDir = 0f;

        bool hasSideContact = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                hasSideContact = true;
                break;
            }
        }

        if (!hasSideContact) return false;

        float horizontalOffset = transform.position.x - collision.transform.position.x;
        if (Mathf.Abs(horizontalOffset) < 0.01f) return false;

        pushDir = Mathf.Sign(horizontalOffset);
        return true;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return ;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
