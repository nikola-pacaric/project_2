using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private int meleeDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject fireSlashPrefab;
    [SerializeField] private float attackRangeX = 1f;
    [SerializeField] private float attackRangeY = 1f;
    [SerializeField] private LayerMask enemyLayers;

    private PlayerControls controls;

    [SerializeField] private float attackCooldown = 0.5f;
    private bool canAttack = true;


    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Melee_Attack.performed += ctx => MeleeAttack();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }
    private void OnDisable()
    {
        controls.Player.Disable();
    }

    void MeleeAttack()
    {
        if (!canAttack) return;
        if (fireSlashPrefab != null)
        {
            GameObject instance = Instantiate(fireSlashPrefab, attackPoint.position, Quaternion.identity, transform);

            SlashAttack sa = instance.GetComponent<SlashAttack>();
            if (sa != null)
                sa.playerCombat = this;

            Animator instanceAnim = instance.GetComponent<Animator>();
            if (instanceAnim != null)
                instanceAnim.SetTrigger("PlaySlash");

            if (TryGetComponent<PlayerController>(out PlayerController pc))
                pc.LockFacing(attackCooldown);

            AudioManager.Instance?.PlaySFX(SfxId.Slash);

            Destroy(instance, attackCooldown);
        }

        StartCoroutine(AttackCooldownRoutine());
    }

    public void ApplyDamage(Vector2 position)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(position, new Vector2(attackRangeX, attackRangeY), 0f, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(meleeDamage);
            }
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
        

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 1));
    }
}
