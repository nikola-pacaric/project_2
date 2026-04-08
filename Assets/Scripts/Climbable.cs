using UnityEngine;

/// <summary>
/// Marker component — attach to any GameObject with a trigger Collider2D
/// to make it a climbable zone (stairs, ladder, rope, etc.).
/// PlayerController detects this component via OnTriggerEnter/Exit2D.
/// </summary>
public class Climbable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Climbable] ENTER — hit by: {other.name} on layer: {LayerMask.LayerToName(other.gameObject.layer)}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[Climbable] EXIT — left by: {other.name}");
    }
}
