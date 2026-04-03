using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public int maxHearts = 3;
    public int segmentsPerHeart = 4;
    public int currentSegment;

    public Vector2 startingPossPoint;
    public Vector2 respawnPoint;

    [Header("Damage Effects")]
    private SpriteRenderer sprite;
    private Color originalColor;


    void Start()
    {
        currentSegment = maxHearts * segmentsPerHeart;
        respawnPoint = transform.position;
        startingPossPoint = transform.position;

        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;
    }

    public void GainHeart()
    {
        maxHearts++;
        currentSegment = maxHearts * segmentsPerHeart;
        FindAnyObjectByType<HeartsUI>().AddHeart();
    }

    public void RespawnAfterFall()
    {
        int remainder = currentSegment % segmentsPerHeart;

        if (remainder == 0)
        {
            currentSegment -= segmentsPerHeart;
        }
        else
        {
            currentSegment -= remainder;
        }
        currentSegment = Mathf.Max(currentSegment, 0);

        transform.position = respawnPoint;
        if(currentSegment <= 0)
        {
            GameOver();
        }
    }

    public void RespawnAfterEnvironmentDamage(int segmentsLost)
    {
        currentSegment -= segmentsLost;
        currentSegment = Mathf.Max (currentSegment, 0);

        transform.position = respawnPoint;

        if(currentSegment <= 0)
        {
            GameOver();
        }
    }

    public void TakeEnemyDamage(int segmentsLost)
    {
        currentSegment -= segmentsLost;
        currentSegment = Mathf.Max(currentSegment, 0);

        StartCoroutine(FlashRed());

        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("isHurt");

        ApplyKnockback();

        if (currentSegment <= 0)
        {
            GameOver();
        }
    }

    IEnumerator FlashRed()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sprite.color = originalColor;
    }

    void ApplyKnockback()
    {
        GetComponent<PlayerController>().LockMovement(0.3f); // Lock movement for 0.3 seconds

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float dir = transform.localScale.x > 0 ? -1 : 1; // Knockback in opposite direction of facing

        rb.linearVelocity = Vector2.zero; // Reset current velocity
        rb.linearVelocity = new Vector2(dir * 9f, 9f); // Adjust knockback strength as needed

    }

    public void Heal(int segmentsGained)
    {
        currentSegment = Mathf.Min(currentSegment + segmentsGained, maxHearts * segmentsPerHeart);
    }

    private void GameOver()
    {
        transform.position = startingPossPoint;
        Debug.Log("Game Over! Restart required.");
        //Load main menu or restart Arc_1 scene
    }

    void Update()
    {
        
    }
}
