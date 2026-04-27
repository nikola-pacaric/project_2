using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private static Checkpoint lastHealed;

    private bool activated;

    public static void ResetLastHealed() => lastHealed = null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.respawnPoint = transform.position;

                bool canHeal = lastHealed != this;
                if (canHeal && ph.currentSegment % ph.segmentsPerHeart != 0)
                {
                    int maxSegments = ph.maxHearts * ph.segmentsPerHeart;
                    ph.Heal(maxSegments - ph.currentSegment);
                    lastHealed = this;
                }

                if (!activated)
                {
                    activated = true;
                    AudioManager.Instance?.PlaySFX(SfxId.Checkpoint);
                }
            }
        }
    }
}
