using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 3;
    [SerializeField] private float invulnerabilityDuration = 0.15f;

    [Header("Events")]
    [SerializeField] private UnityEvent<DamageInfo> onDamaged = new();
    [SerializeField] private UnityEvent onHPChanged = new();
    [SerializeField] private UnityEvent onDied = new();

    public UnityEvent<DamageInfo> OnDamaged => onDamaged;
    public UnityEvent OnHPChanged => onHPChanged;
    public UnityEvent OnDied => onDied;

    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public bool IsDead { get; private set; }

    private float lastDamageTime = -999f;

    private void Awake() => CurrentHP = maxHP;

    public bool ReceiveHit(DamageInfo info)
    {
        if (IsDead) return false;
        if (Time.time - lastDamageTime < invulnerabilityDuration) return false;

        lastDamageTime = Time.time;
        CurrentHP = Mathf.Max(0, CurrentHP - info.amount);

        onDamaged.Invoke(info);
        onHPChanged.Invoke();

        if (CurrentHP <= 0)
        {
            IsDead = true;
            onDied.Invoke();
        }
        return true;
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        onHPChanged.Invoke();
    }

    public void LoseHP(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        onHPChanged.Invoke();

        if (CurrentHP <= 0)
        {
            IsDead = true;
            onDied.Invoke();
        }
    }

    public void SetMaxHP(int newMax, bool refill)
    {
        maxHP = Mathf.Max(1, newMax);
        if (refill) CurrentHP = maxHP;
        else CurrentHP = Mathf.Min(CurrentHP, maxHP);
        onHPChanged.Invoke();
    }

    public void RestoreState(int newMaxHP, int newCurrentHP)
    {
        maxHP = Mathf.Max(1, newMaxHP);
        CurrentHP = Mathf.Clamp(newCurrentHP, 0, maxHP);
        IsDead = CurrentHP <= 0;
        onHPChanged.Invoke();
    }
}
