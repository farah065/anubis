using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    public float FollowSpeed;
    public float PatrolSpeed;
    public float AngularSpeed;
    public float Acceleration;
    public float StoppingDistance;

    public float MaxHp;
    public float Damage;
    public float DetectionRange;
    public float AttackRange;
    public float PatrolRadius;
}
