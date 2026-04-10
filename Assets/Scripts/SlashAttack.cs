using UnityEngine;

public class SlashAttack : MonoBehaviour
{
    public PlayerCombat playerCombar;

    public void DealDamage()
    {
        playerCombar.ApplyDamage(transform.position);
    }
}
