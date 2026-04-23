using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject fireSlashPrefab;
    [SerializeField] private float attackCooldown = 0.5f;

    private PlayerControls controls;
    private bool canAttack = true;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Melee_Attack.performed += ctx => MeleeAttack();
    }

    private void OnEnable()  => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void MeleeAttack()
    {
        if (!canAttack || fireSlashPrefab == null) return;

        GameObject instance = Instantiate(fireSlashPrefab, attackPoint.position, Quaternion.identity, transform);

        if (instance.TryGetComponent<Animator>(out Animator instanceAnim))
            instanceAnim.SetTrigger("PlaySlash");

        if (TryGetComponent<PlayerController>(out PlayerController pc))
            pc.LockFacing(attackCooldown);

        AudioManager.Instance?.PlaySFX(SfxId.Slash);
        Destroy(instance, attackCooldown);

        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
