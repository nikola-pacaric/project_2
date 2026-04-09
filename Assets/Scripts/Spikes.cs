using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerHealth>(out PlayerHealth ph))
        {
            ph.TakeSpikeDamage(damage);
        }
    }
}
