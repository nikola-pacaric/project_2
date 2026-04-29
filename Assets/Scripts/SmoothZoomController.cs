using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Smoothly lerps the Cinemachine ortho size while PixelPerfectCamera stays enabled.
// PPC drives Camera.orthographicSize in its own LateUpdate from refResolutionY / (2 * assetsPPU);
// this controller's LateUpdate runs after PPC's (DefaultExecutionOrder = 10000) and overrides
// the value during a zoom. On completion the target size is baked back into refResolution so
// the override can release without a snap.
[DefaultExecutionOrder(10000)]
public class SmoothZoomController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private bool useUnscaledTime = true;

    private Camera mainCam;
    private PixelPerfectCamera ppc;
    private float baseAspect = 16f / 9f;

    private bool overriding;
    private float overrideSize;

    private Coroutine zoomCoroutine;

    private void Awake()
    {
        mainCam = Camera.main;
        if (mainCam != null) mainCam.TryGetComponent(out ppc);
        if (ppc != null && ppc.refResolutionY > 0)
            baseAspect = ppc.refResolutionX / (float)ppc.refResolutionY;
    }

    public Coroutine ZoomTo(float targetSize, float duration)
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomRoutine(targetSize, duration));
        return zoomCoroutine;
    }

    private IEnumerator ZoomRoutine(float targetSize, float duration)
    {
        if (vcam == null) yield break;

        float startSize = vcam.Lens.OrthographicSize;
        overrideSize = startSize;
        overriding = true;

        if (duration > 0f)
        {
            float t = 0f;
            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                float eased = Mathf.SmoothStep(0f, 1f, u);
                overrideSize = Mathf.Lerp(startSize, targetSize, eased);
                yield return null;
            }
        }

        overrideSize = targetSize;
        BakeIntoPpc(targetSize);
        vcam.Lens.OrthographicSize = targetSize;
        overriding = false;
    }

    private void LateUpdate()
    {
        if (!overriding || mainCam == null || vcam == null) return;
        vcam.Lens.OrthographicSize = overrideSize;
        mainCam.orthographicSize = overrideSize;
    }

    private void BakeIntoPpc(float orthoSize)
    {
        if (ppc == null) return;
        int newRefY = Mathf.Max(2, Mathf.RoundToInt(orthoSize * 2f * ppc.assetsPPU));
        int newRefX = Mathf.Max(2, Mathf.RoundToInt(newRefY * baseAspect));
        ppc.refResolutionX = newRefX;
        ppc.refResolutionY = newRefY;
    }
}
