using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Smoothly drives Cinemachine ortho size and TargetOffset.x.
// PPC is disabled for the duration of a zoom so the lerp can flow freely
// (PPC otherwise locks orthographicSize to refResolutionY / 2 / assetsPPU
// every frame). On finish we set PPC.refResolution to the chosen preset and
// re-enable PPC. Because the target ortho is derived from the preset itself
// (refResolutionY / 2 / assetsPPU), PPC re-enables on the exact value with
// no fight between Cinemachine Brain and PPC.
public class CameraZoomHelper : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;

    [Header("Transition")]
    [Tooltip("Seconds for a full zoom or offset transition.")]
    [SerializeField] private float zoomDuration = 3f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Zoom Presets (1080p-divisible refResolution)")]
    [Tooltip("Reference resolutions PPC will use at each zoom tier. Each entry must integer-divide 1080p so PPC can upscale cleanly. SetZoom(index) picks one of these.")]
    [SerializeField] private Vector2Int[] zoomPresets = new[]
    {
        new Vector2Int(960, 540),  // 1080p / 2 — most zoomed out
        new Vector2Int(640, 360),  // 1080p / 3
        new Vector2Int(480, 270),  // 1080p / 4 — project default
        new Vector2Int(384, 216),  // 1080p / 5
        new Vector2Int(320, 180),  // 1080p / 6 — most zoomed in
    };

    private Coroutine _zoomCoroutine;
    private Coroutine _offsetCoroutine;
    private CinemachinePositionComposer _composer;
    private Camera _mainCam;
    private PixelPerfectCamera _pixelPerfect;

    private void Awake()
    {
        if (vcam != null) _composer = vcam.GetComponent<CinemachinePositionComposer>();

        _mainCam = Camera.main;
        if (_mainCam != null) _mainCam.TryGetComponent(out _pixelPerfect);
    }

    public void SetZoom(int presetIndex)
    {
        if (zoomPresets == null || zoomPresets.Length == 0)
        {
            Debug.LogWarning("[CameraZoomHelper] No zoom presets configured.", this);
            return;
        }
        if (presetIndex < 0 || presetIndex >= zoomPresets.Length)
        {
            Debug.LogWarning($"[CameraZoomHelper] Preset index {presetIndex} out of range (0..{zoomPresets.Length - 1}).", this);
            return;
        }

        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(ZoomRoutine(zoomPresets[presetIndex]));
    }

    public void SetOffsetX(float x)
    {
        if (_offsetCoroutine != null) StopCoroutine(_offsetCoroutine);
        _offsetCoroutine = StartCoroutine(OffsetRoutine(x));
    }

    private IEnumerator ZoomRoutine(Vector2Int targetRef)
    {
        if (vcam == null) yield break;

        // Target ortho is derived directly from the preset so PPC can re-enable
        // on the exact same value (refY / 2 / assetsPPU) — no rounding mismatch.
        float ppu = _pixelPerfect != null ? _pixelPerfect.assetsPPU : 16f;
        float targetSize = targetRef.y / (2f * ppu);

        bool ppcWasEnabled = _pixelPerfect != null && _pixelPerfect.enabled;
        if (ppcWasEnabled) _pixelPerfect.enabled = false;

        float startSize = vcam.Lens.OrthographicSize;

        if (zoomDuration > 0f)
        {
            float t = 0f;
            while (t < zoomDuration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = Mathf.Clamp01(t / zoomDuration);
                float eased = Mathf.SmoothStep(0f, 1f, u);
                float size = Mathf.Lerp(startSize, targetSize, eased);
                vcam.Lens.OrthographicSize = size;
                if (_mainCam != null) _mainCam.orthographicSize = size;
                yield return null;
            }
        }

        vcam.Lens.OrthographicSize = targetSize;
        if (_mainCam != null) _mainCam.orthographicSize = targetSize;

        if (_pixelPerfect != null)
        {
            _pixelPerfect.refResolutionX = targetRef.x;
            _pixelPerfect.refResolutionY = targetRef.y;
            if (ppcWasEnabled) _pixelPerfect.enabled = true;
        }
    }

    private IEnumerator OffsetRoutine(float targetX)
    {
        if (_composer == null) yield break;

        float startX = _composer.TargetOffset.x;

        if (zoomDuration > 0f)
        {
            float t = 0f;
            while (t < zoomDuration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = Mathf.Clamp01(t / zoomDuration);
                float eased = Mathf.SmoothStep(0f, 1f, u);
                Vector3 offset = _composer.TargetOffset;
                offset.x = Mathf.Lerp(startX, targetX, eased);
                _composer.TargetOffset = offset;
                yield return null;
            }
        }

        Vector3 final = _composer.TargetOffset;
        final.x = targetX;
        _composer.TargetOffset = final;
    }
}
