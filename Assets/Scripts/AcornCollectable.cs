using UnityEngine;

public class AcornCollectable : MonoBehaviour
{
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.1f;

    private Animator animator;
    private Collider2D col;
    private Vector3 startPos;
    private bool collected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (collected) return;
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        AudioManager.Instance?.PlaySFX(SfxId.CommonPickup);

        if (animator != null)
        {
            animator.SetTrigger("Collected");
        }

        if (col != null)
        {
            col.enabled = false;
        }
    }

    public void DestroyAcorn()
    {
        Destroy(gameObject);
    }
}
