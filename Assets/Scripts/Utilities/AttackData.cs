using UnityEngine;

namespace GEM
{
    public class AttackData : MonoBehaviour
    {
        public float attackDamage = 0;
        public float knockbackForce = 0;

        public void Initialize(float damage, float knockback)
        {
            attackDamage = damage;
            knockbackForce = knockback;
        }
    }
}