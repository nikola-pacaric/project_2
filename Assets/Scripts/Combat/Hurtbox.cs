using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField] private Health health;

    [Tooltip("If true, the hit lands (attacker gets the OnHitLanded callback so a stomp still bounces) " +
             "but no damage is forwarded to Health — no HP loss, no red flash, no invulnerability tick. " +
             "Use for trampoline-style stompable surfaces (e.g. mushroom hat).")]
    [SerializeField] private bool bouncyOnly = false;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (health == null) health = GetComponentInParent<Health>();
    }

    public bool ReceiveHit(DamageInfo info)
    {
        if (bouncyOnly) return true;
        if (health == null) return false;
        return health.ReceiveHit(info);
    }
}
