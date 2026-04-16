using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private int scoreValue = 50;

    [Header("Visual Effects")]
    public GameObject deathAnimationPrefab;
    private SpriteRenderer sprite;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            originalColor = sprite.color;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = originalColor;
    }

    private void Die()
    {
        if (deathAnimationPrefab != null)
        {
            Instantiate(deathAnimationPrefab, transform.position, Quaternion.identity);
        }
        GameManager.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
