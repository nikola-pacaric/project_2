using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.respawnPoint = transform.position;

                if (!activated)
                {
                    activated = true;
                    AudioManager.Instance?.PlaySFX(SfxId.Checkpoint);
                }
            }
        }
    }
}
