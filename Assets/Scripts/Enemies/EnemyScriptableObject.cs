using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Navmesh Agent Settings")]
    public float FollowSpeed;
    public float PatrolSpeed;
    public float AngularSpeed;
    public float Acceleration;
    public float StoppingDistance;

    [Header("Enemy Stats")]
    public float MaxHp;
    public float Damage;
    public float AttackCooldown;

    [Header("Enemy Ranges")]
    public float PlayerDetectionRadius;
    public float AttackZoneRadius;
    public float PatrolRadius;
}
