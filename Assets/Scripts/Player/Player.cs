using System;
using UnityEngine;

namespace GEM
{
    public class Player : MonoBehaviour
    {
        public float health = 100;
        [SerializeField] private GameManager gameManager;

        private void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in scene!");
            }
        }

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
                Vector3 forceDirection = (transform.position - other.transform.root.position).normalized; //using transform.root here to get the transform of the player (parent), this may cause issues with other damage dealing objects
                forceDirection.y = 0f;
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
            if (health <= 0)
            {
                gameManager.ReturnToCooldownRoom();
            }
        }

    }
}