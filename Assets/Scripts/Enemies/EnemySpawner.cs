using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private Mummy _zombiePrefab;
    private IObjectPool<Mummy> _zombiePool;

    public override void Awake()
    {
        base.Awake();
        _zombiePool = new ObjectPool<Mummy>(CreateEnemy, OnGet, OnRelease);
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
        Mummy enemy = Instantiate(_zombiePrefab);
        enemy.SetPool(_zombiePool);
        return enemy;
    }

    private void OnGet(Mummy enemy)
    {
        enemy.gameObject.SetActive(true);
        Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        enemy.transform.position = randomSpawnPoint.position;
    }

    private void OnRelease(Mummy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    public void SpawnZombie()
    { 
        _zombiePool.Get();
    }
}
