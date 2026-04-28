using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class EndingCinematic : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Camera")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private Transform cameraFocus;
    [SerializeField] private float cameraMoveDuration = 2.5f;
    [SerializeField] private float zoomTargetSize = 3f;

    [Header("Dim Overlay")]
    [SerializeField] private CanvasGroup dimGroup;
    [SerializeField, Range(0f, 1f)] private float dimTargetAlpha = 0.65f;
    [SerializeField] private float dimFadeDuration = 2.5f;

    [Header("Message")]
    [SerializeField] private CanvasGroup messageGroup;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageHoldDuration = 3f;
    [SerializeField] private float typewriterCharsPerSecond = 25f;

    [Header("Audio")]
    [SerializeField] private float musicFadeDuration = 2f;

    [Header("Game Over Handoff")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private float handoffFadeDuration = 0.3f;

    private bool triggered;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        if (dimGroup != null) dimGroup.alpha = 0f;
        if (messageGroup != null) messageGroup.alpha = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        StartCoroutine(PlayCinematic(other.gameObject));
    }

    private IEnumerator PlayCinematic(GameObject player)
    {
        RunTimer.Instance?.Pause();

        if (player.TryGetComponent<PlayerController>(out PlayerController controller))
            controller.enabled = false;
        if (player.TryGetComponent<PlayerCombat>(out PlayerCombat combat))
            combat.enabled = false;

        if (player.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
            playerRb.linearVelocity = Vector2.zero;

        if (player.TryGetComponent<Animator>(out Animator playerAnim))
            playerAnim.SetFloat("Speed", 0f);

        if (vcam != null)
        {
            vcam.Follow = null;
            vcam.LookAt = null;
        }

        // PixelPerfectCamera locks ortho size every frame; disable it for the
        // duration of the cinematic so the smooth zoom/pan can take effect.
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.TryGetComponent<PixelPerfectCamera>(out PixelPerfectCamera ppc))
            ppc.enabled = false;

        AudioManager.Instance?.FadeOutMusic(musicFadeDuration);

        Coroutine cam = StartCoroutine(MoveCamera());
        Coroutine dim = StartCoroutine(FadeCanvas(dimGroup, 0f, dimTargetAlpha, dimFadeDuration));

        if (cam != null) yield return cam;
        if (dim != null) yield return dim;

        if (messageGroup != null) messageGroup.alpha = 1f;
        yield return TypewriterReveal();
        yield return new WaitForSecondsRealtime(messageHoldDuration);

        StartCoroutine(FadeCanvas(dimGroup, dimGroup != null ? dimGroup.alpha : 0f, 0f, handoffFadeDuration));
        StartCoroutine(FadeCanvas(messageGroup, messageGroup != null ? messageGroup.alpha : 0f, 0f, handoffFadeDuration));

        if (gameOverUI != null)
        {
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            gameOverUI.Show(score);
        }
    }

    private IEnumerator MoveCamera()
    {
        if (vcam == null || cameraFocus == null) yield break;

        Vector3 startPos = vcam.transform.position;
        Vector3 endPos = new Vector3(cameraFocus.position.x, cameraFocus.position.y, startPos.z);
        float startSize = vcam.Lens.OrthographicSize;

        float t = 0f;
        while (t < cameraMoveDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / cameraMoveDuration);
            float eased = Mathf.SmoothStep(0f, 1f, u);
            vcam.transform.position = Vector3.Lerp(startPos, endPos, eased);
            vcam.Lens.OrthographicSize = Mathf.Lerp(startSize, zoomTargetSize, eased);
            yield return null;
        }
        vcam.transform.position = endPos;
        vcam.Lens.OrthographicSize = zoomTargetSize;
    }

    private IEnumerator TypewriterReveal()
    {
        if (messageText == null) yield break;

        messageText.ForceMeshUpdate();
        int total = messageText.textInfo.characterCount;
        if (total == 0) yield break;

        messageText.maxVisibleCharacters = 0;

        float charsPerSecond = Mathf.Max(1f, typewriterCharsPerSecond);
        float interval = 1f / charsPerSecond;
        float t = 0f;
        int shown = 0;

        while (shown < total)
        {
            t += Time.unscaledDeltaTime;
            int target = Mathf.Min(total, Mathf.FloorToInt(t / interval));
            if (target > shown)
            {
                shown = target;
                messageText.maxVisibleCharacters = shown;
            }
            yield return null;
        }
        messageText.maxVisibleCharacters = total;
    }

    private IEnumerator FadeCanvas(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        group.alpha = to;
    }
}
