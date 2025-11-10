using UnityEngine;
using UnityEngine.AI;

public interface IEnemy
{
    EnemyScriptableObject EnemyData { get; }
    GameObject Target { get; }
    NavMeshAgent Agent { get; }
    void Initialise();
    void Attack();
    void TakeDamage(float damage);
    void Die();
}
