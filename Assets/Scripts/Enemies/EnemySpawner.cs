using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemySpawner : Singleton<EnemySpawner>
{
    public int NumberOfEnemies = 0;
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Mummy Settings")]
    [SerializeField] private Mummy _mummyPrefab;
    [SerializeField] private int _groupsToSpawn = 2;
    [SerializeField] private float _spawnRadius = 2f;  // radius around spawn point
    [SerializeField] private int _minEnemies = 2;
    [SerializeField] private int _maxEnemies = 5;
    [SerializeField] private float _minDistanceBetweenEnemies = 1f; // min spacing to prevent overlap

    [Header("Debug")]
    [SerializeField] private bool _isManualSpawningEnabled = false;

    private IObjectPool<Mummy> _mummyPool;

    #region monobehaviour methods
    public override void Awake()
    {
        base.Awake();
        // create enemy pools
        _mummyPool = new ObjectPool<Mummy>(CreateMummy, OnGetMummy, OnReleaseMummy);
    }

    private void Start()
    {
        for (int i = 0; i < _groupsToSpawn; i++)
        {
            SpawnMummies();
        }
    }

    private void Update()
    {
        // only for testing purposes
        if (Input.GetKeyDown(KeyCode.Z) && _isManualSpawningEnabled)
        {
            SpawnMummies();
        }
    }
    #endregion

    #region private methods
    #region mummies
    private Mummy CreateMummy()
    {
        Mummy enemy = Instantiate(_mummyPrefab);
        enemy.SetPool(_mummyPool);
        enemy.enabled = true;
        return enemy;
    }

    private void OnGetMummy(Mummy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemy.enabled = true;
    }

    private void OnReleaseMummy(Mummy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void SpawnMummies()
    {
        Debug.Log("Spawning Mummies: ");
        // get a random spawn point and enemy count
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        int enemyCount = Random.Range(_minEnemies, _maxEnemies + 1);

        // keep track of spawned positions to avoid overlap
        List<Vector3> spawnedPositions = new List<Vector3>();
        for (int i = 0; i < enemyCount; i++)
        {
            if (TryGetValidSpawnPosition(spawnPoint.position, spawnedPositions, _minDistanceBetweenEnemies, out Vector3 spawnPos))
            {
                Mummy enemy = _mummyPool.Get();
                enemy.transform.position = spawnPos;
                enemy.InitialPosition = spawnPos;
                spawnedPositions.Add(spawnPos);
                NumberOfEnemies++;
            }
        }
    }
    #endregion

    private bool TryGetValidSpawnPosition(Vector3 center, List<Vector3> existingPositions, float minDistanceBetweenEnemies, out Vector3 validPosition)
    {
        validPosition = center;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * _spawnRadius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

            // get closest point on navmesh
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 navPos = hit.position;

                // check distance from other spawned enemies
                bool tooClose = false;
                foreach (var pos in existingPositions)
                {
                    if (Vector3.Distance(navPos, pos) < minDistanceBetweenEnemies)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    validPosition = navPos;
                    return true;
                }
            }
        }

        return NavMesh.SamplePosition(center, out NavMeshHit fallbackHit, 1.0f, NavMesh.AllAreas)
            ? (validPosition = fallbackHit.position) != Vector3.zero
            : false;
    }
    #endregion
}
