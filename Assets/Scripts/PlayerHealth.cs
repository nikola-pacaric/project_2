using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    public static event System.Action OnHealthChanged;

    [SerializeField] private Health health;
    [SerializeField] private int segmentsPerHeartValue = 4;

    [Header("Knockback")]
    [SerializeField] private float enemyKnockbackX = 9f;
    [SerializeField] private float enemyKnockbackY = 9f;
    [SerializeField] private float spikeBounceForce = 9f;
    [SerializeField] private float spikeHorizontalForce = 5f;
    [SerializeField] private float knockbackLockDuration = 0.3f;

    [Header("Visual")]
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Game Over")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private float gameOverDelay = 1.5f;

    public Vector2 respawnPoint;

    // Legacy public surface — kept for HeartsUI, GameManager, Checkpoint compat
    public int maxHearts => Mathf.Max(1, health.MaxHP / segmentsPerHeartValue);
    public int currentSegment => health.CurrentHP;
    public int segmentsPerHeart => segmentsPerHeartValue;

    private SpriteRenderer sprite;
    private Color originalColor;
    private PlayerController pc;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        sprite = GetComponent<SpriteRenderer>();
        pc = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (sprite != null) originalColor = sprite.color;
    }

    private void OnEnable()
    {
        health.OnDamaged.AddListener(HandleDamaged);
        health.OnHPChanged.AddListener(HandleHPChanged);
        health.OnDied.AddListener(HandleDied);
    }

    private void OnDisable()
    {
        health.OnDamaged.RemoveListener(HandleDamaged);
        health.OnHPChanged.RemoveListener(HandleHPChanged);
        health.OnDied.RemoveListener(HandleDied);
    }

    private void Start()
    {
        respawnPoint = transform.position;
        OnHealthChanged?.Invoke();
    }

    private void HandleHPChanged() => OnHealthChanged?.Invoke();

    private void HandleDamaged(DamageInfo info)
    {
        AudioManager.Instance?.PlaySFX(SfxId.Hit);
        StartCoroutine(FlashRed());
        if (anim != null) anim.SetTrigger("isHurt");
        ApplyKnockback(info);
    }

    private IEnumerator FlashRed()
    {
        if (sprite == null) yield break;
        sprite.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        sprite.color = originalColor;
    }

    private void ApplyKnockback(DamageInfo info)
    {
        if (pc != null) pc.LockMovement(knockbackLockDuration);
        if (rb == null) return;
        rb.linearVelocity = Vector2.zero;

        if (info.type == DamageType.Spike)
        {
            float horizontalInput = pc != null ? pc.MoveInputX : 0f;
            rb.linearVelocity = new Vector2(horizontalInput * spikeHorizontalForce, spikeBounceForce);
        }
        else
        {
            float dir = transform.position.x >= info.sourcePosition.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * enemyKnockbackX, enemyKnockbackY);
        }
    }

    private void HandleDied()
    {
        if (isDead) return;
        isDead = true;
        AudioManager.Instance?.PlaySFX(SfxId.PlayerDeath);
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(gameOverDelay);
        if (gameOverUI != null && GameManager.Instance != null)
            gameOverUI.Show(GameManager.Instance.Score);
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public void GainHeart()
    {
        health.SetMaxHP(health.MaxHP + segmentsPerHeartValue, refill: true);
    }

    public void RespawnAfterFall()
    {
        int remainder = health.CurrentHP % segmentsPerHeartValue;
        int lose = remainder == 0 ? segmentsPerHeartValue : remainder;
        health.LoseHP(lose);
        transform.position = respawnPoint;
    }

    public void RespawnAfterEnvironmentDamage(int segmentsLost)
    {
        health.LoseHP(segmentsLost);
        transform.position = respawnPoint;
    }

    public void Heal(int segmentsGained) => health.Heal(segmentsGained);

    public void RestoreState(int restoredMaxHearts, int restoredCurrentSegment)
    {
        health.RestoreState(restoredMaxHearts * segmentsPerHeartValue, restoredCurrentSegment);
    }
}
