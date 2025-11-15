using UnityEngine;
using System.Collections;
using GEM;
using UnityEngine.Pool;

public class Mummy : Enemy
{
    [SerializeField] private Transform _hipTransform;
    private IObjectPool<Mummy> _mummyPool;
    [SerializeField] private AttackData meleeAttackData;

    protected override void Initialise()
    {
        base.Initialise();
        meleeAttackData.attackDamage = _enemyData.Damage;
        _hipTransform.localPosition = new Vector3(0, 0.84f, 0);
        _hipTransform.localRotation = new Quaternion(0.085506916f, -0.00255261548f, 0.0296848305f, 0.995891988f);
    }

    protected override void Die()
    {
        base.Die();
        StartCoroutine(ReleaseCoroutine());
    }

    public void SetPool(IObjectPool<Mummy> pool)
    {
        _mummyPool = pool;
    }

    private IEnumerator ReleaseCoroutine()
    {
        // wait until death feedbacks have finished playing
        while (_deathFeedbacks.IsPlaying)
        {
            yield return null;
        }
        EnemySpawner.Instance.NumberOfEnemies--;
        if (EnemySpawner.Instance.NumberOfEnemies == 0)
        {
            GameManager.Instance.SpawnPowerup();
        }

        _mummyPool.Release(this);
    }
}
