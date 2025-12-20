using GEM;
using UnityEngine;

public class Seth : Enemy
{
    [SerializeField] private AttackData meleeAttackData;

    protected override void OnEnable()
    {
        Initialise();
        CurrentState = EnemyState.Following;
    }

    protected override void Update()
    {
        if (_knockbackTimeRemaining > 0f && _knockbackVelocity.sqrMagnitude > 0.0001f)
        {
            transform.position += _knockbackVelocity * Time.deltaTime;
            _knockbackTimeRemaining -= Time.deltaTime;
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

    protected override void Initialise()
    {
        base.Initialise();
        meleeAttackData.attackDamage = _enemyData.Damage;
    }
}
