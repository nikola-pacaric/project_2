using UnityEngine;

public class ParallaxLayers : MonoBehaviour
{

    public Transform cameraTransform;
    public float parallaxFactor = 0.5f;
    // 1 = background follows camera vertically at full speed (no drift, always fills screen).
    // Lower values create a depth effect but require a taller sprite to avoid gaps.
    public float parallaxFactorY = 1f;

    private float length;
    private float posX, posY;
    private float prevCamX, prevCamY;

    void Start()
    {
        posX = transform.position.x;
        posY = transform.position.y;
        prevCamX = cameraTransform.position.x;
        prevCamY = cameraTransform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // Use per-frame delta so the formula is correct regardless of where the camera starts
        float dx = cameraTransform.position.x - prevCamX;
        float dy = cameraTransform.position.y - prevCamY;
        prevCamX = cameraTransform.position.x;
        prevCamY = cameraTransform.position.y;

        posX += dx * parallaxFactor;
        posY += dy * parallaxFactorY;

        transform.position = new Vector3(posX, posY, transform.position.z);

        // Infinite horizontal tiling: shift by one sprite width when camera drifts past it
        float relX = cameraTransform.position.x - posX;
        if (relX > length)       posX += length;
        else if (relX < -length) posX -= length;
    }
}
