using UnityEngine;
using UnityEngine.AI;

public interface IEnemy
{
    EnemyScriptableObject EnemyData { get; }
    Vector3 Target { get; }
    NavMeshAgent Agent { get; }
    void Initialise();
    void Attack();
    void TakeDamage(float damage);
    void Die();
}
