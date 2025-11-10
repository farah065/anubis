using UnityEngine;
using UnityEngine.Pool;

public class Mummy : Enemy
{
    private IObjectPool<Mummy> _zombiePool;

    protected override void Die()
    {
        base.Die();
        _zombiePool.Release(this);
    }

    public void SetPool(IObjectPool<Mummy> pool)
    {
        _zombiePool = pool;
    }
}
