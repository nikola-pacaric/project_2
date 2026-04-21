using UnityEngine;

public class SlashAttack : MonoBehaviour
{
    public PlayerCombat playerCombat;

    public void DealDamage()
    {
        playerCombat.ApplyDamage(transform.position);
    }
}
