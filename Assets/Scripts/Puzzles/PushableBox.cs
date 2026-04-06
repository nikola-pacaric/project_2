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

    private Rigidbody2D rb;
    private bool isBeingPushed = false;
    private PlayerController pusherController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.linearDamping = drag;

        if (boxSprite == null) boxSprite = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (!isBeingPushed)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        isBeingPushed = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();

        if (pc != null || !pc.isGrounded) return;

        Vector2 pushDir = Vector2.zero;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f){
                pushDir = new Vector2(-contact.normal.x, 0f);
                break;
            }
        }

        if (pushDir == Vector2.zero) return;

        rb.linearVelocity = new Vector2(pushDir.x * pushSpeed, rb.linearVelocity.y);
        isBeingPushed = true;

        UpdateVisual(true);
        pusherController = pc;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        UpdateVisual(false);
        pusherController = null; 
    }

    private void UpdateVisual(bool pushed)
    {
        if (boxSprite != null)
        {
            boxSprite.color = pushed ? pushedColor : normalColor;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
