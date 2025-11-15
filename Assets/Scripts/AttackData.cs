using UnityEngine;

namespace GEM
{
    public class AttackData : MonoBehaviour
    {
        public int attackDamage;
        public float knockbackForce;

        public void Initialize(int damage, float knockback)
        {
            attackDamage = damage;
            knockbackForce = knockback;
        }
    }
}