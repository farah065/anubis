using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrolling,
    Following,
    Attacking,
    Dying
}

public abstract class Enemy : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Animator Animator;
    [HideInInspector] public Vector3 InitialPosition;

    [SerializeField] protected EnemyScriptableObject _enemyData;
    [SerializeField] protected EnemyState _currentState;
    [SerializeField] protected SphereCollider _detectionCollider;

    protected GameObject _targetGameObj;
    protected float _currentHp;

    protected void OnEnable()
    {
        Initialise();
        StartCoroutine(PatrolRoutine());
    }

    protected void Update()
    {
        if (_currentState == EnemyState.Following && _targetGameObj != null)
        {
            Agent.SetDestination(_targetGameObj.transform.position);
        }

        float targetSpeed = 0f;
        if (Agent.hasPath && Agent.remainingDistance > Agent.stoppingDistance)
        {
            targetSpeed = _currentState == EnemyState.Following ? _enemyData.FollowSpeed : _enemyData.PatrolSpeed;
        }

        Agent.speed = Mathf.MoveTowards(Agent.speed, targetSpeed, Agent.acceleration * Time.deltaTime);

        Animator.SetFloat("speed", Agent.speed);

        // Check for attack range
        if (_currentState == EnemyState.Following && _targetGameObj != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _targetGameObj.transform.position);
            if (distanceToTarget <= _enemyData.AttackZoneRadius)
            {
                Attack();
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // raycast to check line of sight
            Vector3 dir = (other.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position + Vector3.up * 1.3f, new Vector3(dir.x, 0, dir.z));
            Debug.DrawRay(ray.origin, ray.direction * _enemyData.PlayerDetectionRadius, Color.red, 1.0f);

            if (Physics.Raycast(ray, out RaycastHit hit, _enemyData.PlayerDetectionRadius))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    _targetGameObj = other.gameObject;
                    _currentState = EnemyState.Following;
                }
            }
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if (_currentState != EnemyState.Following)
        {
            if (other.CompareTag("Player"))
            {
                // raycast to check line of sight
                Vector3 dir = (other.transform.position - transform.position).normalized;
                Ray ray = new Ray(transform.position + Vector3.up * 1.4f, new Vector3(dir.x, 0, dir.z));
                Debug.DrawRay(ray.origin, ray.direction * _enemyData.PlayerDetectionRadius, Color.red, 1.0f);

                if (Physics.Raycast(ray, out RaycastHit hit, _enemyData.PlayerDetectionRadius))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        _targetGameObj = other.gameObject;
                        _currentState = EnemyState.Following;
                    }
                }
            }
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _enemyData.PlayerDetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyData.AttackZoneRadius);

        if (Agent.hasPath)
        {
            for (int i = 0; i < Agent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(Agent.path.corners[i], Agent.path.corners[i + 1], Color.blue);
            }
        }
    }

    protected void Initialise()
    {
        Agent.speed = _enemyData.FollowSpeed;
        Agent.speed = _enemyData.PatrolSpeed;
        Agent.angularSpeed = _enemyData.AngularSpeed;
        Agent.acceleration = _enemyData.Acceleration;
        Agent.stoppingDistance = _enemyData.StoppingDistance;

        _currentHp = _enemyData.MaxHp;
        _detectionCollider.radius = _enemyData.PlayerDetectionRadius;
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;

        for (int i = 0; i < 30; i++)
        {
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // Check that the hit point is still within the patrol radius
                if (Vector3.Distance(center, hit.position) <= range)
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = center;
        return false;
    }

    protected void Patrol()
    {
        // pick a random point on the navmesh within patrol radius
        Vector3 point;
        if (RandomPoint(InitialPosition, _enemyData.PatrolRadius, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
            Agent.SetDestination(point);
        }
    }

    private IEnumerator PatrolRoutine()
    {
        _currentState = EnemyState.Patrolling;
        bool _isWaiting = false;

        while (_currentState == EnemyState.Patrolling)
        {
            if (!_isWaiting)
            {
                Patrol();
                _isWaiting = true;
            }

            // Wait until the agent reaches destination
            yield return new WaitUntil(() => !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance);

            // Wait a bit at the patrol point
            yield return new WaitForSeconds(Random.Range(1f, 3f));

            _isWaiting = false;
        }
    }

    protected void TakeDamage(float damage)
    {
        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Die();
        }
    }
    protected virtual void Attack()
    {
        Animator.SetTrigger("attack");
        _currentState = EnemyState.Attacking;
        StartCoroutine(AttackCoroutine());
    }

    protected IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(_enemyData.AttackCooldown);
        _currentState = EnemyState.Following;
    }

    protected virtual void Die()
    {
        Debug.Log("Enemy died");
    }
}
