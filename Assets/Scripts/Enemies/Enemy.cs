using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected EnemyScriptableObject _enemyData;
    [SerializeField] protected NavMeshAgent _agent;
    [SerializeField] protected Animator _animator;

    protected GameObject _target;
    protected float _currentHp;
    protected Vector3 _initialPosition;

    protected void OnEnable()
    {
        Initialise();
    }

    protected void Update()
    {
        if (_target != null)
        {
            _agent.SetDestination(_target.transform.position);
        }

        //if (_agent.remainingDistance <= _agent.stoppingDistance) //done with path
        //{
        //    Vector3 point;
        //    if (RandomPoint(_initialPosition, 10, out point)) //pass in our centre point and radius of area
        //    {
        //        Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
        //        _agent.SetDestination(point);
        //    }
        //}

        _animator.SetFloat("speed", _agent.velocity.magnitude);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _target = other.gameObject;
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _enemyData.DetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyData.AttackRange);
    }

    protected void Initialise()
    {
        _agent.speed = _enemyData.FollowSpeed;
        //_agent.speed = _enemyData.PatrolSpeed;
        _agent.angularSpeed = _enemyData.AngularSpeed;
        _agent.acceleration = _enemyData.Acceleration;
        _agent.stoppingDistance = _enemyData.StoppingDistance;

        _currentHp = _enemyData.MaxHp;
        _initialPosition = transform.position;
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; // random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
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
