using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrolling,
    Waiting,
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

    protected GameObject _target;
    protected float _currentHp;

    protected void OnEnable()
    {
        Initialise();
        StartCoroutine(PatrolRoutine());
    }

    protected void Update()
    {
        if (_currentState == EnemyState.Following && _target != null)
        {
            Agent.SetDestination(_target.transform.position);
        }

        float targetSpeed = 0f;
        if (Agent.hasPath && Agent.remainingDistance > Agent.stoppingDistance)
        {
            targetSpeed = _currentState == EnemyState.Following ? _enemyData.FollowSpeed : _enemyData.PatrolSpeed;
        }

        Agent.speed = Mathf.MoveTowards(Agent.speed, targetSpeed, Agent.acceleration * Time.deltaTime);

        Animator.SetFloat("speed", Agent.speed);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _target = other.gameObject;
            _currentState = EnemyState.Following;
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _enemyData.DetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyData.AttackRange);

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
        Debug.Log("PATROL");
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

    }

    protected virtual void Die()
    {
        Debug.Log("Enemy died");
    }
}
