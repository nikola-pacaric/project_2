using UnityEngine;

[DefaultExecutionOrder(10000)]
public class ParallaxLayers : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float parallaxFactor = 0.5f;
    [SerializeField] private float offsetY = 0f;

    [Header("Zoom Scaling")]
    [Tooltip("Scale the sprite uniformly with camera ortho size, so zooming out doesn't reveal the empty background.")]
    [SerializeField] private bool scaleWithZoom = true;
    [SerializeField] private Camera zoomCamera;

    private SpriteRenderer _sr;
    private Vector3 _baseScale;
    private float _baseOrthoSize = 1f;
    private float _posX;
    private float _prevCamX;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;

        if (zoomCamera == null) zoomCamera = Camera.main;
        if (zoomCamera != null && zoomCamera.orthographicSize > 0f)
            _baseOrthoSize = zoomCamera.orthographicSize;

        _posX = transform.position.x;
        _prevCamX = cameraTransform.position.x;
    }

    private void LateUpdate()
    {
        if (scaleWithZoom && zoomCamera != null && _baseOrthoSize > 0f)
        {
            float scale = zoomCamera.orthographicSize / _baseOrthoSize;
            transform.localScale = _baseScale * scale;
        }

        float currentLength = _sr != null ? _sr.bounds.size.x : 0f;

        float dx = cameraTransform.position.x - _prevCamX;
        _prevCamX = cameraTransform.position.x;
        _posX += dx * parallaxFactor;

        transform.position = new Vector3(_posX, cameraTransform.position.y + offsetY, transform.position.z);

        // Infinite horizontal tiling — uses scaled bounds so tiling distance grows with zoom.
        if (currentLength > 0f)
        {
            float relX = cameraTransform.position.x - _posX;
            if (relX > currentLength)       _posX += currentLength;
            else if (relX < -currentLength) _posX -= currentLength;
        }
    }
}
