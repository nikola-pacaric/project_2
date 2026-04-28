using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraZoomHelper : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private float zoomSpeed = 0.5f;

    private Coroutine _zoomCoroutine;
    private Coroutine _offsetCoroutine;
    private CinemachinePositionComposer _composer;
    private PixelPerfectCamera _pixelPerfect;
    private float _baseAspect = 16f / 9f;

    private void Awake()
    {
        _composer = vcam.GetComponent<CinemachinePositionComposer>();

        Camera mainCam = Camera.main;
        if (mainCam != null) _pixelPerfect = mainCam.GetComponent<PixelPerfectCamera>();
        if (_pixelPerfect != null && _pixelPerfect.refResolutionY > 0)
            _baseAspect = _pixelPerfect.refResolutionX / (float)_pixelPerfect.refResolutionY;
    }

    public void SetZoom(float size)
    {
        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(ZoomRoutine(size));
    }

    public void SetOffsetX(float x)
    {
        if (_offsetCoroutine != null) StopCoroutine(_offsetCoroutine);
        _offsetCoroutine = StartCoroutine(OffsetRoutine(x));
    }

    private IEnumerator ZoomRoutine(float targetSize)
    {
        float startSize = vcam.Lens.OrthographicSize;
        float t = 0f;

        while (Mathf.Abs(vcam.Lens.OrthographicSize - targetSize) > 0.01f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime * zoomSpeed);
            float size = Mathf.Lerp(startSize, targetSize, t);
            ApplyOrthoSize(size);
            yield return null;
        }

        ApplyOrthoSize(targetSize);
    }

    private IEnumerator OffsetRoutine(float targetX)
    {
        if (_composer == null) yield break;

        float startX = _composer.TargetOffset.x;
        float t = 0f;

        while (Mathf.Abs(_composer.TargetOffset.x - targetX) > 0.01f)
        {
            t += Time.deltaTime * zoomSpeed;
            Vector3 offset = _composer.TargetOffset;
            offset.x = Mathf.Lerp(startX, targetX, t);
            _composer.TargetOffset = offset;
            yield return null;
        }

        Vector3 final = _composer.TargetOffset;
        final.x = targetX;
        _composer.TargetOffset = final;
    }

    // PixelPerfectCamera locks the camera's orthographic size every frame based on
    // refResolution / assetsPPU. Writing to vcam.Lens.OrthographicSize alone is a
    // tug-of-war with PPC. Instead, derive the required refResolution from the
    // desired ortho size and set both — PPC then re-derives the same ortho size
    // and pixel-perfect rendering is preserved at every zoom level.
    private void ApplyOrthoSize(float orthoSize)
    {
        vcam.Lens.OrthographicSize = orthoSize;

        if (_pixelPerfect == null) return;

        int newRefY = Mathf.Max(2, Mathf.RoundToInt(orthoSize * 2f * _pixelPerfect.assetsPPU));
        int newRefX = Mathf.Max(2, Mathf.RoundToInt(newRefY * _baseAspect));
        _pixelPerfect.refResolutionX = newRefX;
        _pixelPerfect.refResolutionY = newRefY;
    }
}
