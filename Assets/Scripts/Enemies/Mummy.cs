using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AI;

public class Mummy : MonoBehaviour, IEnemy
{
    public EnemyScriptableObject EnemyData { get => _enemyData; }
    public GameObject Target { get => _target; }
    public NavMeshAgent Agent { get => _navMeshAgent; }

    [SerializeField] private EnemyScriptableObject _enemyData;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private Animator _animator;
    private GameObject _target;
    private IObjectPool<Mummy> _zombiePool;
    private float _currentHp;
    private Vector3 _initialPosition;

    private void OnEnable()
    {
        Initialise();
    }

    private void Update()
    {
        if (Target != null)
        {
            Agent.SetDestination(Target.transform.position);
        }
        _animator.SetFloat("speed", Agent.velocity.magnitude);
    }

    public void Initialise()
    {
        _currentHp = EnemyData.MaxHp;
        _initialPosition = transform.position;
    }

    public void Attack()
    {

    }

    public void Partol()
    {
        // walk to a random point around initial position
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += _initialPosition;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _target = other.gameObject;
        }
    }
}
