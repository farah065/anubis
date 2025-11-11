using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Mummy _mummyPrefab;
    private IObjectPool<Mummy> _mummyPool;

    [SerializeField] private float _spawnRadius = 2f;  // radius around spawn point
    [SerializeField] private int _minEnemies = 2;
    [SerializeField] private int _maxEnemies = 5;
    [SerializeField] private float _minDistanceBetweenEnemies = 1f; // min spacing to prevent overlap

    public override void Awake()
    {
        base.Awake();
        _mummyPool = new ObjectPool<Mummy>(CreateEnemy, OnGet, OnRelease);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SpawnZombies();
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
        enemy.gameObject.SetActive(true);
    }

    private void OnRelease(Mummy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    public void SpawnZombies()
    {
        // pick a random spawn point
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        // pick a random number of enemies
        int enemyCount = Random.Range(_minEnemies, _maxEnemies + 1);

        // keep track of already placed enemy positions
        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition(spawnPoint.position, spawnedPositions);
            Mummy enemy = _mummyPool.Get();
            enemy.transform.position = spawnPos;
            enemy.InitialPosition = spawnPos;

            spawnedPositions.Add(spawnPos);
        }
    }

    private Vector3 GetValidSpawnPosition(Vector3 center, List<Vector3> existingPositions)
    {
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // pick a random point within the spawn radius
            Vector2 offset = Random.insideUnitCircle * _spawnRadius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

            // check distance from existing enemies
            bool tooClose = false;
            foreach (var pos in existingPositions)
            {
                if (Vector3.Distance(candidate, pos) < _minDistanceBetweenEnemies)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return candidate;
        }

        // if we can't find a good spot, just return the center
        return center;
    }
}
