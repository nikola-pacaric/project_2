using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZoomHelper : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private float zoomSpeed = 0.5f;

    private Coroutine _zoomCoroutine;
    private Coroutine _offsetCoroutine;
    private CinemachinePositionComposer _composer;

    private void Awake()
    {
        _composer = vcam.GetComponent<CinemachinePositionComposer>();
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
            t += Time.deltaTime * zoomSpeed;
            vcam.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        vcam.Lens.OrthographicSize = targetSize;
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
}
