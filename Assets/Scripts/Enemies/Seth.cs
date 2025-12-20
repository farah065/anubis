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

    protected override void Initialise()
    {
        base.Initialise();
        meleeAttackData.attackDamage = _enemyData.Damage;
    }

    protected override void Die()
    {
        base.Die();
        Animator.Play("Die");
    }
}
