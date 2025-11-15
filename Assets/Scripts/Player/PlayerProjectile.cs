using System.Collections;
using System.Collections.Generic;
using GEM;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private float speed;
    private float maxDistance;
    [SerializeField] private AttackData rangedAttackData;

    private Vector3 _startPosition;
    private bool _initialized;
    [SerializeField]private SphereCollider _explosionCollider;
    public void Initialize(float damage, float knockback, float speed, float maxDistance, float explosionRadius)
    {
        rangedAttackData.attackDamage = damage;
        rangedAttackData.knockbackForce = knockback;
        this.speed = speed;
        this.maxDistance = maxDistance;
        _startPosition = transform.position;
        _initialized = true;

        _explosionCollider.radius = explosionRadius;
        _explosionCollider.enabled = false;
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

    private void OnTriggerEnter(Collider other)
    {
        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        StartCoroutine(EnableColliderTemporarily());
        Destroy(gameObject);
    }

    private IEnumerator EnableColliderTemporarily()
    {
        _explosionCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
    }
}