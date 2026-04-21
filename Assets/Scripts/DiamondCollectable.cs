using UnityEngine;

public class DiamondCollectable : MonoBehaviour
{

    [SerializeField] private int scoreValue = 200;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.GainHeart();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            AudioManager.Instance?.PlaySFX(SfxId.DiamondPickup);
            animator.SetTrigger("Collected");
            GetComponent<Collider2D>().enabled = false;
        }
    }

    public void DestroyDiamond()
    {
        Destroy(gameObject);
    }
}
