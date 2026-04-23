using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private int scoreValue = 50;

    [Header("Visual Effects")]
    [SerializeField] private GameObject deathAnimationPrefab;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Hitstun")]
    [Tooltip("Contact Hitboxes to disable briefly on hit so the enemy can't damage the player while flinching.")]
    [SerializeField] private Hitbox[] contactHitboxes;
    [SerializeField] private float hitstunDuration = 0.15f;

    private SpriteRenderer sprite;
    private Color originalColor;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;
    }

    private void OnEnable()
    {
        health.OnDamaged.AddListener(HandleDamaged);
        health.OnDied.AddListener(HandleDied);
    }

    private void OnDisable()
    {
        health.OnDamaged.RemoveListener(HandleDamaged);
        health.OnDied.RemoveListener(HandleDied);
    }

    private void HandleDamaged(DamageInfo info)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
        if (contactHitboxes != null && contactHitboxes.Length > 0)
            StartCoroutine(HitstunRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (sprite == null) yield break;
        sprite.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        sprite.color = originalColor;
    }

    private IEnumerator HitstunRoutine()
    {
        foreach (Hitbox hb in contactHitboxes)
            if (hb != null) hb.SetActive(false);

        yield return new WaitForSeconds(hitstunDuration);

        foreach (Hitbox hb in contactHitboxes)
            if (hb != null) hb.SetActive(true);
    }

    private void HandleDied()
    {
        if (deathAnimationPrefab != null)
            Instantiate(deathAnimationPrefab, transform.position, Quaternion.identity);

        AudioManager.Instance?.PlaySFXAt(SfxId.EnemyDeath, transform.position);

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        Destroy(gameObject);
    }
}
