using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    public float Speed;
    public float MaxHp;
    public float Damage;
    public float DetectionRange;
    public float AttackRange;
}
