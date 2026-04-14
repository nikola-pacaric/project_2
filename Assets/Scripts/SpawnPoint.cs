using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string id;

    public string Id => id;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.2f);
    }
}
