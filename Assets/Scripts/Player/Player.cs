using System;
using UnityEngine;

namespace GEM
{
    public class Player : Singleton<Player>
    {
        public float health = 100;

        public void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Name: {other.gameObject.name}, Tag: {other.gameObject.tag}");
            if (other.CompareTag("PlayerAttack"))
            {
                return;
            }
            AttackData attack = other.GetComponent<AttackData>();
            if (attack != null)
            {
                Vector3 forceDirection = (transform.position - other.transform.root.position).normalized;
                forceDirection.y = 0;
                float damage = attack.attackDamage;
                Vector3 force = forceDirection * attack.knockbackForce;
                OnHit(damage, force);
            }
        }

        public virtual void OnHit(float damage, Vector3 force)
        {
            TakeDamage(damage, force);
            Debug.Log("Player hit!");
        }

        protected virtual void TakeDamage(float damage, Vector3 force)
        {
            Debug.Log($"HP was: {health}");
            health -= damage;
            Debug.Log($"HP now: {health}");
        }



    }
}