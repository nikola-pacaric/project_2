using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    [SerializeField] private int amount = 1;
    [SerializeField] private DamageType type = DamageType.Contact;
    [SerializeField] private GameObject sourceOverride;

    [Tooltip("If true, each Hurtbox can only be hit once per SetActive(true) cycle. " +
             "Use for swing-type attacks (slash, stomp). Leave off for persistent " +
             "hitboxes that should re-tick (gas cloud, enemy body contact).")]
    [SerializeField] private bool singleHitPerActivation = false;

    public event Action<Hurtbox> OnHitLanded;

    public int Amount => amount;
    public DamageType Type => type;

    private Collider2D col;
    private GameObject source;
    private bool initialized;
    private readonly HashSet<Hurtbox> hitTargets = new();

    private void Awake() => EnsureInitialized();

    private void EnsureInitialized()
    {
        if (initialized) return;
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        source = sourceOverride != null ? sourceOverride : transform.root.gameObject;
        initialized = true;
    }

    public void SetActive(bool active)
    {
        EnsureInitialized();
        if (col != null) col.enabled = active;
        if (active) hitTargets.Clear();
    }

    public void SetAmount(int newAmount) => amount = newAmount;

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnTriggerStay2D(Collider2D other) => TryHit(other);

    private void TryHit(Collider2D other)
    {
        if (!other.TryGetComponent<Hurtbox>(out Hurtbox hurtbox)) return;
        if (hurtbox.transform.root == transform.root) return;
        if (singleHitPerActivation && hitTargets.Contains(hurtbox)) return;

        DamageInfo info = new DamageInfo
        {
            amount = amount,
            type = type,
            source = source,
            sourcePosition = transform.position
        };

        if (hurtbox.ReceiveHit(info))
        {
            if (singleHitPerActivation) hitTargets.Add(hurtbox);
            OnHitLanded?.Invoke(hurtbox);
        }
    }
}
