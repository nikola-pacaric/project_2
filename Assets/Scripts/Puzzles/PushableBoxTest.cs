using UnityEngine;

public class PushableBoxTest : MonoBehaviour
{
    [SerializeField] private float pushSpeed = 3f;
    [SerializeField] private float minInputToPush = 0.1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        if (pc == null || !pc.isGrounded) return;
        if (!TryGetPushDirection(collision, out float pushDir)) return;

        float inputX = pc.MoveInputX;
        if (Mathf.Abs(inputX) < minInputToPush) return;
        if (Mathf.Sign(inputX) != Mathf.Sign(pushDir)) return;

        rb.linearVelocity = new Vector2(pushDir * pushSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
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
}
