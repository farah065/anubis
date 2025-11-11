using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private Mummy _mummyPrefab;
    private IObjectPool<Mummy> _mummyPool;

    public override void Awake()
    {
        base.Awake();
        _mummyPool = new ObjectPool<Mummy>(CreateEnemy, OnGet, OnRelease);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SpawnZombie();
        }
    }

    private Mummy CreateEnemy()
    {
        Mummy enemy = Instantiate(_mummyPrefab);
        enemy.SetPool(_mummyPool);
        return enemy;
    }

    private void OnGet(Mummy enemy)
    {
        Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        enemy.transform.position = randomSpawnPoint.position;
        enemy.gameObject.SetActive(true);
        enemy.InitialPosition = randomSpawnPoint.position;
    }

    private void OnRelease(Mummy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    public void SpawnZombie()
    { 
        _mummyPool.Get();
    }
}
