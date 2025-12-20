using System;
using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace GEM
{
    public class Player : Singleton<Player>
    {
        public float health = 100;
        public float maxHealth = 100;
        public float baseHealth = 100;
        public float healthBonus = 0;
        public float blockFactor = 2f;
        public GameObject parryArcHitbox;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MMF_Player hitFeedbacks;
        [SerializeField] private MMF_Player parryFeedbacks;
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        private bool _isInvulnerable = false;
        private Coroutine _activeParryCoroutine = null;

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

                // disable the attack hitbox after hitting the player to prevent multiple hits
                other.enabled = false;
            }
        }

        public virtual void OnHit(float damage, Vector3 force)
        {
            var current = PlayerStateMachine.Instance.CurrentState;

            // If we are in the parry window, immediately reset the parry cooldown
            // (allowing parry again) and play the parry arc visual.
            if (current is BlockingState blocking && blocking.IsInParryWindow)
            {
                PlayerStateMachine.Instance.ResetParryCooldown();
                // Stop any existing parry coroutine before starting a new one
                if (_activeParryCoroutine != null)
                {
                    StopCoroutine(_activeParryCoroutine);
                    if (parryArcHitbox != null) parryArcHitbox.SetActive(false);
                }
                _activeParryCoroutine = StartCoroutine(Parry());
                return;
            }

            // If invulnerable (from previous hit or parry), ignore damage
            if (_isInvulnerable)
            {
                Debug.Log("Player is invulnerable; hit ignored.");
                return;
            }

            // Blocking reduces incoming damage
            if (current is BlockingState)
            {
                damage /= blockFactor; 
            }

            TakeDamage(damage, force);
            hitFeedbacks?.PlayFeedbacks();
            Debug.Log("Player hit!");
        }

        private IEnumerator Parry()
        {
            Debug.Log("Parry successful! No damage taken.");
            StartCoroutine(InvulnerabilityWindow());
            yield return new WaitForSeconds(0.05f);
            if (parryArcHitbox != null) parryArcHitbox.SetActive(true);
            parryFeedbacks?.PlayFeedbacks();
            yield return new WaitForSeconds(0.3f);
            if (parryArcHitbox != null) parryArcHitbox.SetActive(false);
            _activeParryCoroutine = null;
        }

        protected virtual void TakeDamage(float damage, Vector3 force)
        {
            health -= damage;
            Debug.Log($"HP now: {health}");

            // Start invulnerability window after taking damage
            StartCoroutine(InvulnerabilityWindow());
            HealthbarController.Instance.UpdateHealthUI();
            if (health <= 0)
            {
                gameManager.ReturnToCooldownRoom();
            }
        }

        private IEnumerator InvulnerabilityWindow()
        {
            _isInvulnerable = true;
            yield return new WaitForSeconds(invulnerabilityDuration);
            _isInvulnerable = false;
        }

        public void ApplyMaxHealthPowerup(float value)
        {
            healthBonus += value;
            maxHealth = maxHealth + (maxHealth * (healthBonus / 100));
            health = health + (health * (healthBonus / 100));
            HealthbarController.Instance.UpdateHealthUI();
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