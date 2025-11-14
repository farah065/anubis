using UnityEngine;
using UnityEngine.Pool;

public class Mummy : Enemy
{
    private IObjectPool<Mummy> _mummyPool;

    protected override void Die()
    {
        base.Die();
        _mummyPool.Release(this);
    }

    public void SetPool(IObjectPool<Mummy> pool)
    {
        _mummyPool = pool;
    }
}
