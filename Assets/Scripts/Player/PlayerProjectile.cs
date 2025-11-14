using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private float speed;
    private float maxDistance;
    private float explosionRadius;
    [SerializeField] private LayerMask enemyLayerMask;

    private Vector3 _startPosition;
    private bool _initialized;
    private Collider _impactCollider; // collider we actually hit (for zero-radius)

    public void Initialize(float speed, float maxDistance, float explosionRadius, LayerMask enemyLayerMask)
    {
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.explosionRadius = explosionRadius;
        this.enemyLayerMask = enemyLayerMask;
        _startPosition = transform.position;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;

        transform.position += transform.forward * (speed * Time.deltaTime);

        if ((transform.position - _startPosition).sqrMagnitude >= maxDistance * maxDistance)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _impactCollider = collision.collider;
        Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        _impactCollider = other;
        Explode();
    }

    private void Explode()
    {
        if (explosionRadius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<TestEnemy>();
                if (enemy != null) enemy.OnHit();
            }
        }
        else
        {
            if (_impactCollider != null && _impactCollider.gameObject.layer == enemyLayerMask)
            {
                var enemy = _impactCollider.GetComponent<TestEnemy>();
                if (enemy != null) enemy.OnHit();
            }
        }

        Destroy(gameObject);
    }
}