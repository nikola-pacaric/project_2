using System.Collections;
using UnityEngine;

public class GasCloud : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float spawnDuration = 0.4f;
    [SerializeField] private float travelDuration = 3f;
    [SerializeField] private float disspiateDuration = 0.5f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private Animator anim;
    private float direction = 1f;
    private bool isMoving = false;
    private bool isDissipating = false;

    public void Launch(float dir)
    {
        direction = dir;
        transform.localScale = new Vector3(dir, 1f, 1f);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDissipating) return;

        if (other.TryGetComponent<PlayerHealth>(out PlayerHealth ph))
        {
            ph.TakeEnemyDamage(damage);
            Dissipate();
            return;
        }

        // Dissipate on ground or walls — ignore other triggers and enemies
        if (!other.isTrigger && !other.CompareTag("Enemy"))
            Dissipate();
    }

    private void Dissipate()
    {
        if (isDissipating) return;
        isDissipating = true;
        isMoving = false;
        StopAllCoroutines();
        anim.SetTrigger("Hit");
        Destroy(gameObject, disspiateDuration);
    }
}
