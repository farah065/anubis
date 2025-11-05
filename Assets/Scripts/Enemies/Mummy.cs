using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AI;

public class Mummy : MonoBehaviour, IEnemy
{
    public EnemyScriptableObject EnemyData { get => _enemyData; }
    public Vector3 Target { get => _target; }
    public NavMeshAgent Agent { get => _navMeshAgent; }

    [SerializeField] private EnemyScriptableObject _enemyData;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    private Vector3 _target;
    private IObjectPool<Mummy> _zombiePool;
    private float _currentHp;

    private void OnEnable()
    {
        Initialise();
    }

    private void Update()
    {
        // if player in detection range, set target to player position
        // use a sphere overlap check to detect player
        Collider[] hits = Physics.OverlapSphere(transform.position, EnemyData.DetectionRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                _target = hit.transform.position;
                _navMeshAgent.SetDestination(_target);
                break;
            }
        }
    }

    public void Initialise()
    {
        _currentHp = EnemyData.MaxHp;
    }

    public void Attack()
    {

    }

    public void TakeDamage(float damage)
    {
        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        _zombiePool.Release(this);
    }

    public void SetPool(IObjectPool<Mummy> pool)
    {
        _zombiePool = pool;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, EnemyData.DetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, EnemyData.AttackRange);
    }
}
