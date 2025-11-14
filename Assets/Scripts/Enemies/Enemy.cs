using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using MoreMountains.Feedbacks;

public enum EnemyState
{
    Patrolling,
    Following,
    Dying
}

public abstract class Enemy : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Animator Animator;
    public Vector3 InitialPosition;
    public EnemyState CurrentState;

    [SerializeField] protected EnemyScriptableObject _enemyData;
    [SerializeField] protected SphereCollider _detectionCollider;

    [Header("Feedbacks")]
    [SerializeField] protected MMF_Player _deathFeedbacks;
    [SerializeField] protected MMF_Player _spawnFeedbacks;

    protected GameObject _targetGameObj;
    protected float _currentHp;
    protected bool _canAttack = true;

    private Coroutine _playerDetectionCoroutine;

    #region monobehaviour methods
    protected void OnEnable()
    {
        // initialise the enemy and start patrolling
        Initialise();
        StartCoroutine(PatrolRoutine());
    }

    protected void OnDisable()
    {
        // stop all coroutines when disabled
        StopAllCoroutines();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            OnHit();
        }
        // constantly update the agent destination if it's following the player
        if (CurrentState == EnemyState.Following && _targetGameObj != null)
        {
            Agent.SetDestination(_targetGameObj.transform.position);
        }

        // smoothly adjust speed for animation
        // note that the enemy's walking speed isn't actually controlled by the Agent.speed variable
        // the speed value only controls the blend tree in the animator
        // the actual movement speed is determined by root motion from the animations
        // if you want to change how fast the enemy moves, adjust the speed of the animations instead
        float targetSpeed = 0f;
        if (Agent.hasPath && Agent.remainingDistance > Agent.stoppingDistance)
        {
            targetSpeed = CurrentState == EnemyState.Following ? _enemyData.FollowSpeed : _enemyData.PatrolSpeed;
        }

        Agent.speed = Mathf.MoveTowards(Agent.speed, targetSpeed, Agent.acceleration * Time.deltaTime);
        Animator.SetFloat("speed", Agent.speed);

        // check if we can attack the player
        if (CurrentState == EnemyState.Following && _targetGameObj != null && _canAttack)
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
            // start checking for line of sight
            _playerDetectionCoroutine = StartCoroutine(PlayerDetectionCoroutine(other));
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // stop checking for line of sight
            StopCoroutine(_playerDetectionCoroutine);
        }
    }

    protected void OnDrawGizmos()
    {
        // player detection and attack zones
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _enemyData.PlayerDetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyData.AttackZoneRadius);

        // navmesh path
        if (Agent.hasPath)
        {
            for (int i = 0; i < Agent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(Agent.path.corners[i], Agent.path.corners[i + 1], Color.blue);
            }
        }
    }
    #endregion

    #region public methods
    // TODO: hook this up with player
    public virtual void OnHit()
    {
        Debug.Log("Taken hit");
        TakeDamage(10);
    }
    #endregion

    #region protected methods
    protected virtual void Initialise()
    {
        _spawnFeedbacks?.PlayFeedbacks();

        // set agent parameters from scriptable object
        Agent.speed = _enemyData.FollowSpeed;
        Agent.speed = _enemyData.PatrolSpeed;
        Agent.angularSpeed = _enemyData.AngularSpeed;
        Agent.acceleration = _enemyData.Acceleration;
        Agent.stoppingDistance = _enemyData.StoppingDistance;

        // initialise the hp
        _currentHp = _enemyData.MaxHp;
        // ensure the detection collider radius matches the scriptable object's detection radius
        _detectionCollider.radius = _enemyData.PlayerDetectionRadius;
    }

    protected IEnumerator PatrolRoutine()
    {
        CurrentState = EnemyState.Patrolling;

        while (CurrentState == EnemyState.Patrolling)
        {
            Patrol();

            // Wait until the agent reaches destination
            yield return new WaitUntil(() => Agent.remainingDistance <= Agent.stoppingDistance);

            // Wait a bit at the patrol point
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    protected void Patrol()
    {
        // pick a random point on the navmesh within patrol radius
        Vector3 point;

        // if a valid point is found, set it as the agent's destination
        if (RandomPoint(InitialPosition, _enemyData.PatrolRadius, out point))
        {
            Agent.SetDestination(point);

            // draw a ray at the point for debugging
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
        }
    }

    // TODO: figure out why this gets points way out of range sometimes
    protected bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        // get a random point within a sphere of given range
        Vector3 randomPoint = center + Random.insideUnitSphere * range;

        // 30 attempts to find a valid point on the navmesh
        for (int i = 0; i < 30; i++)
        {
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // check that the hit point is still within the patrol radius
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

    protected IEnumerator PlayerDetectionCoroutine(Collider other)
    {
        while (true)
        {
            // raycast to check line of sight
            Vector3 dir = (other.transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position + Vector3.up * 1.3f, new Vector3(dir.x, 0, dir.z));
            Debug.DrawRay(ray.origin, ray.direction * _enemyData.PlayerDetectionRadius, Color.red, 1.0f);

            if (Physics.Raycast(ray, out RaycastHit hit, _enemyData.PlayerDetectionRadius))
            {
                // if we hit a player, set them as the agent's target and change state to following instead of patrolling
                if (hit.collider.CompareTag("Player"))
                {
                    _targetGameObj = other.gameObject;
                    CurrentState = EnemyState.Following;
                    yield break;
                }
            }
            
            // only check every 0.1 seconds
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected IEnumerator AttackCoroutine()
    {
        _canAttack = false;
        yield return new WaitForSeconds(_enemyData.AttackCooldown);
        _canAttack = true;
    }

    #region virtual
    protected virtual void TakeDamage(float damage)
    {
        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Attack()
    {
        // base behaviour is triggering the attack animation and starting the cooldown
        // the animation itself has events to enable/disable the attack collider
        Animator.SetTrigger("attack");
        StartCoroutine(AttackCoroutine());
    }

    // TODO: add death animation and loot drop
    protected virtual void Die()
    {
        CurrentState = EnemyState.Dying;
        _deathFeedbacks?.PlayFeedbacks();
    }
    #endregion
    #endregion
}
