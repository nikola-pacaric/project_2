using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelTransition : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnPointId;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    private bool triggered;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (!other.TryGetComponent(out PlayerHealth player)) return;

        triggered = true;
        StartCoroutine(TransitionRoutine(player));
    }

    private IEnumerator TransitionRoutine(PlayerHealth player)
    {
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(fadeDuration);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState(player, targetSpawnPointId);
        }

        SceneManager.LoadScene(targetScene);
    }
}
