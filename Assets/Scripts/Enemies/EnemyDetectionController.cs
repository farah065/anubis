using System;
using UnityEngine;

public class EnemyDetectionController : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;
    [SerializeField] private float _scanInterval = 0.1f;
    [SerializeField] private LayerMask _playerLayerMask;

    private float _detectionRadius;
    private bool _playerInRange;
    private Collider _currentPlayerCollider;

    protected void OnEnable()
    {
        _detectionRadius = _enemy.GetEnemyData().PlayerDetectionRadius;
        _playerInRange = false;
        _currentPlayerCollider = null;
    }

    protected void Update()
    {
        ScanForPlayer();
    }

    private void ScanForPlayer()
    {
        // spherical scan around this controller's position
        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRadius, _playerLayerMask);

        if (hits.Length > 0)
        {
            Collider playerCollider = Array.Find(hits, hit => hit.CompareTag("Player"));

            if (!_playerInRange)
            {
                _playerInRange = true;
                _currentPlayerCollider = playerCollider;
                _enemy.OnDetectionEnter(playerCollider);
            }
        }
        else if (_playerInRange)
        {
            // player was in range, but is no longer detected
            _playerInRange = false;
            if (_currentPlayerCollider != null)
            {
                _enemy.OnDetectionExit(_currentPlayerCollider);
                _currentPlayerCollider = null;
            }
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float radius = _detectionRadius > 0f && Application.isPlaying
            ? _detectionRadius
            : (_enemy != null ? _enemy.GetEnemyData().PlayerDetectionRadius : 0f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
