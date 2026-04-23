using UnityEngine;

public class HealthTester : MonoBehaviour
{
    private Health health;
    private PlayerHealth playerHealth;

    void Start()
    {
        health = GetComponent<Health>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            DealTestDamage(1);

        if (Input.GetKeyDown(KeyCode.J))
            DealTestDamage(2);

        if (Input.GetKeyDown(KeyCode.K))
            playerHealth.RespawnAfterFall();

        if (Input.GetKeyDown(KeyCode.L))
            playerHealth.GainHeart();
    }

    private void DealTestDamage(int amount)
    {
        health.ReceiveHit(new DamageInfo
        {
            amount = amount,
            type = DamageType.Contact,
            source = gameObject,
            sourcePosition = transform.position
        });
    }
}
