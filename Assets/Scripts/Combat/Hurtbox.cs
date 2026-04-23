using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField] private Health health;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (health == null) health = GetComponentInParent<Health>();
    }

    public bool ReceiveHit(DamageInfo info)
    {
        if (health == null) return false;
        return health.ReceiveHit(info);
    }
}
