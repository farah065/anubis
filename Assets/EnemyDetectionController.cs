using System;
using UnityEngine;

public class EnemyDetectionController : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;
    [SerializeField] private SphereCollider _detectionCollider;

    protected void OnEnable()
    {
        _detectionCollider.radius = _enemy.GetEnemyData().PlayerDetectionRadius;
    }

    protected void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Name: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        if (other.CompareTag("Player"))
        {
            //moved this logic here to seperate the big ass circle collider from the normal hitbox collider
            _enemy.OnDetectionEnter(other);
        }

    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemy.OnDetectionExit(other);
        }
    }

    protected void OnDrawGizmos()
    {
        // player detection
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _detectionCollider.radius);
    }
}
