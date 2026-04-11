using UnityEngine;

public class ParallaxLayers : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float parallaxFactor = 0.5f;
    [SerializeField] private float offsetY = 0f;

    private Camera _cam;
    private float _baseOrthoSize;
    private float _baseLength;
    private float _posX;
    private float _prevCamX;

    private void Start()
    {
        _posX = transform.position.x;
        _prevCamX = cameraTransform.position.x;
        _cam = cameraTransform.GetComponent<Camera>();
        _baseOrthoSize = _cam.orthographicSize;
        _baseLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void LateUpdate()
    {
        float dx = cameraTransform.position.x - _prevCamX;
        _prevCamX = cameraTransform.position.x;
        _posX += dx * parallaxFactor;

        // Scale uniformly to always fill the screen regardless of zoom level
        float scale = _cam.orthographicSize / _baseOrthoSize;
        transform.localScale = new Vector3(scale, scale, 1f);

        // Y always locked to camera — never drifts out vertically
        transform.position = new Vector3(_posX, cameraTransform.position.y + offsetY, transform.position.z);

        // Infinite horizontal tiling — account for current scale
        float length = _baseLength * scale;
        float relX = cameraTransform.position.x - _posX;
        if (relX > length)       _posX += length;
        else if (relX < -length) _posX -= length;
    }
}
