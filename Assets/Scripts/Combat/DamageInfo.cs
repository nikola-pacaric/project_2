using UnityEngine;

public enum DamageType
{
    Stomp,
    Slash,
    Contact,
    Projectile,
    Spike
}

public struct DamageInfo
{
    public int amount;
    public DamageType type;
    public GameObject source;
    public Vector2 sourcePosition;
}
