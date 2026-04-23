using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Hitbox))]
public class GasCloud : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float spawnDuration = 0.4f;
    [SerializeField] private float travelDuration = 3f;
    [SerializeField] private float dissipateDuration = 0.5f;

    [Header("Dissipate On Impact")]
    [SerializeField] private LayerMask groundLayer;

    private Animator anim;
    private Hitbox hitbox;
    private float direction = 1f;
    private bool isMoving = false;
    private bool isDissipating = false;

    public void Launch(float dir)
    {
        direction = dir;
        transform.localScale = new Vector3(dir, 1f, 1f);
    }

    private void Awake()
    {
        hitbox = GetComponent<Hitbox>();
    }

    private void OnEnable()
    {
        if (hitbox != null) hitbox.OnHitLanded += HandleHitLanded;
    }

    private void OnDisable()
    {
        if (hitbox != null) hitbox.OnHitLanded -= HandleHitLanded;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(GasRoutine());
    }

    private void Update()
    {
        if (isMoving && !isDissipating)
            transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
    }

    private IEnumerator GasRoutine()
    {
        anim.Play("gas_alone_spawn");
        yield return new WaitForSeconds(spawnDuration);

        anim.Play("gas_alone_travel");
        isMoving = true;
        yield return new WaitForSeconds(travelDuration);

        Dissipate();
    }

    private void HandleHitLanded(Hurtbox _)
    {
        Dissipate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isMoving || isDissipating) return;
        if (((1 << other.gameObject.layer) & groundLayer) == 0) return;
        Dissipate();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isMoving || isDissipating) return;
        if (((1 << other.gameObject.layer) & groundLayer) == 0) return;
        Dissipate();
    }

    private void Dissipate()
    {
        if (isDissipating) return;
        isDissipating = true;
        isMoving = false;
        if (hitbox != null) hitbox.SetActive(false);
        StopAllCoroutines();
        anim.SetTrigger("Hit");
        Destroy(gameObject, dissipateDuration);
    }
}
