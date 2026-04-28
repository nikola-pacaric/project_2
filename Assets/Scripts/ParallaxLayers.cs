using UnityEngine;

[DefaultExecutionOrder(10000)]
public class ParallaxLayers : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float parallaxFactor = 0.5f;
    [SerializeField] private float offsetY = 0f;

    private float _baseLength;
    private float _posX;
    private float _prevCamX;

    private void Start()
    {
        _posX = transform.position.x;
        _prevCamX = cameraTransform.position.x;
        _baseLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void LateUpdate()
    {
        float dx = cameraTransform.position.x - _prevCamX;
        _prevCamX = cameraTransform.position.x;
        _posX += dx * parallaxFactor;

        transform.position = new Vector3(_posX, cameraTransform.position.y + offsetY, transform.position.z);

        // Infinite horizontal tiling
        float relX = cameraTransform.position.x - _posX;
        if (relX > _baseLength)       _posX += _baseLength;
        else if (relX < -_baseLength) _posX -= _baseLength;
    }
}
