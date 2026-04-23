using UnityEngine;

// Lives on the spawned slash prefab. The Hitbox does the damage; this script
// exists only to toggle the Hitbox on/off from animation events so damage
// is gated to the actual swing frames, not the whole prefab lifetime.
[RequireComponent(typeof(Hitbox))]
public class SlashAttack : MonoBehaviour
{
    private Hitbox hitbox;

    private void Awake()
    {
        hitbox = GetComponent<Hitbox>();
        hitbox.SetActive(false);
    }

    public void ActivateHitbox() => hitbox.SetActive(true);
    public void DeactivateHitbox() => hitbox.SetActive(false);

    // Legacy animation event. Existing slash clip calls DealDamage() at the
    // swing frame — map it to ActivateHitbox so existing anim assets keep
    // working without re-authoring the clip.
    public void DealDamage() => ActivateHitbox();

    // Backwards-compat for any external callers — no-op now.
    public PlayerCombat playerCombat;
}
