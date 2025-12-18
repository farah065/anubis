using System;
using UnityEngine;

namespace GEM
{
    public class Player : Singleton<Player>
    {
        public float health = 100;
        public float maxHealth = 100;
        public float baseHealth = 100;
        public float healthBonus = 0;
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
            if (health <= 0)
            {
                gameManager.ReturnToCooldownRoom();
            }
        }

        public void ApplyMaxHealthPowerup(float value)
        {
            healthBonus += value;
            maxHealth = maxHealth + (maxHealth * (healthBonus / 100));
            health = health + (health * (healthBonus / 100));
        }


        public void ApplyPowerup(PowerupData powerup)
        {
            switch (powerup.property)
            {
                case PlayerProperty.MaxHealth:
                    ApplyMaxHealthPowerup(powerup.value);
                    break;
                case PlayerProperty.MeleeAttackDamage:
                    PlayerStateMachine.Instance.ApplyMeleeAttackDamagePowerup(powerup.value);
                    break;
                case PlayerProperty.MeleeAttackKnockback:
                    PlayerStateMachine.Instance.ApplyMeleeAttackKnockbackPowerup(powerup.value);
                    break;
                case PlayerProperty.RangedAttackDamage:
                    PlayerStateMachine.Instance.ApplyRangedAttackDamagePowerup(powerup.value);
                    break;
                case PlayerProperty.RangedAttackKnockback:
                    PlayerStateMachine.Instance.ApplyRangedAttackKnockbackPowerup(powerup.value);
                    break;
                case PlayerProperty.MovementSpeed:
                    PlayerStateMachine.Instance.ApplyMovementSpeedPowerup(powerup.value);
                    break;
                default:
                    Debug.LogWarning("Unknown powerup property");
                    break;
            }
        }
    }
}