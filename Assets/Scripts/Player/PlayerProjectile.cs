using System.Collections;
using System.Collections.Generic;
using GEM;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private float speed;
    private float maxDistance;
    [SerializeField] private AttackData rangedAttackData;
    [SerializeField] private MMF_Player launchFeedbacks;
    [SerializeField] private MMF_Player flightFeedbacks;
    [SerializeField] private MMF_Player explosionFeedbacks;

    private Vector3 _direction;

    private Vector3 _startPosition;
    private bool _initialized;
    [SerializeField]private SphereCollider _explosionCollider;

    public void Initialize(float damage, float knockback, float speed, float maxDistance, float explosionRadius, Vector3 direction)
    {
        rangedAttackData.attackDamage = damage;
        rangedAttackData.knockbackForce = knockback;
        this.speed = speed;
        this.maxDistance = maxDistance;
        _startPosition = transform.position;
        _direction = direction.normalized;
        _initialized = true;

        _explosionCollider.radius = explosionRadius;
        _explosionCollider.enabled = false;

        launchFeedbacks?.PlayFeedbacks();
        flightFeedbacks?.PlayFeedbacks();
    }

    private void Update()
    {
        if (!_initialized) return;

        transform.position += _direction * (speed * Time.deltaTime);

        if ((transform.position - _startPosition).sqrMagnitude >= maxDistance * maxDistance)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerAttack")) return; 
        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("PlayerAttack")) return; 
        Explode();
    }

    private void Explode()
    {
        flightFeedbacks?.StopFeedbacks();
        explosionFeedbacks?.PlayFeedbacks();
        StartCoroutine(EnableColliderTemporarily());
    }

    private IEnumerator EnableColliderTemporarily()
    {
        _explosionCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}