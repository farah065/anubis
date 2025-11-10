using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    public class PlayerActionController : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayerMask;

        [Header("References")]
        [SerializeField] private PlayerMovementController playerMovementController;
        [SerializeField] private PlayerLookController playerLookController;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput; // required for reading melee attack input

        [Header("Melee Attack Settings")]
        [SerializeField] private float baseMeleeAttackRange = 2f;
        [SerializeField] private float baseMeleeAttackDamage = 10;
        [SerializeField] private float baseMeleeAttackSpeed = 1f;
        [SerializeField] private float baseMeleeAttackKnockback = 5f;
        [SerializeField] private float meleeAttackCooldown = 0.4f; // time between attacks in combo
        [SerializeField] private float comboResetTime = 1.25f; // time before combo resets if no new attack

        [Header("Ranged Attack Settings")]
        [SerializeField] private float baseRangedAttackRange = 10f;
        [SerializeField] private float baseRangedAttackDamage = 8;
        [SerializeField] private float baseRangedAttackSpeed = 1.5f;
        [SerializeField] private float baseRangedAttackArea = 0f;

        [Header("Block Settings")]
        [SerializeField] private float baseBlockEfficiency = 0.5f;
        [SerializeField] private float baseParryTimingWindow = 0.3f;

        // timeout delta-time (repurposed)
        private float _attackDelta;       // per-attack cooldown timer
        private float _attackComboDelta;  // combo continuation window timer
        private int _attackComboNum;      // current combo index (0..2)
        private int MAX_COMBO = 3;

        void Awake()
        {
            if (playerMovementController == null)
            {
                Debug.LogWarning("Player Movement Controller not assigned in Player Action Controller.");
                playerMovementController = FindFirstObjectByType<PlayerMovementController>();
            }

            if (playerLookController == null)
            {
                Debug.LogWarning("Player Look Controller not assigned in Player Action Controller.");
                playerLookController = FindFirstObjectByType<PlayerLookController>();
            }
        }

        private void Update()
        {
            MeleeAttack();
            RangedAttack();
            Block();
        }

        private void MeleeAttack()
        {
            Debug.Log($"Attack Delta: {_attackDelta}, Combo Delta: {_attackComboDelta}, Combo Num: {_attackComboNum}");

            // decrement timers each frame
            if (_attackDelta > 0f) _attackDelta -= Time.deltaTime;
            if (_attackComboDelta > 0f)
            {
                _attackComboDelta -= Time.deltaTime;
                if (_attackComboDelta <= 0f)
                {
                    // combo window expired
                    _attackComboNum = 0;
                }
            }

            // read attack input (assumes an action named "MeleeAttack")
            var meleeAction = playerInput != null ? playerInput.actions.FindAction("Melee Attack") : null;
            bool attackTriggered = meleeAction != null && meleeAction.triggered;

            // only proceed if input triggered and per-attack cooldown finished and combo not maxed out
            if (!attackTriggered || _attackDelta > 0f) return;
            if (_attackComboDelta > 0f && _attackComboNum == MAX_COMBO) return;

            // execute attack
            playerMovementController?.SetIsPerformingAction(true);
            playerMovementController?.SetPlayerRotation(playerLookController.CurrentLookDirection);

            // base stats (placeholders for future modifiers per combo stage)
            float attackRange = baseMeleeAttackRange; // could vary by _attackComboNum
            float attackDamage = baseMeleeAttackDamage; // could scale with combo index
            float attackKnockback = baseMeleeAttackKnockback; // could scale with combo index
            float attackAngle = 90f; // frontal arc
            float attackHeightOffset = 1f;

            Vector3 forward = transform.forward;
            Vector3 attackOrigin = transform.position + Vector3.up * attackHeightOffset;
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange, enemyLayerMask);
            foreach (var hit in hitColliders)
            {
                Debug.Log(hit.gameObject.name);
                Vector3 directionToTarget = (hit.transform.position - attackOrigin).normalized;
                float angle = Vector3.Angle(forward, directionToTarget);
                if (angle <= attackAngle * 0.5f)
                {
                    // TODO: Apply damage & knockback
                    Debug.DrawLine(attackOrigin, hit.transform.position, Color.red, 0.2f);

                    TestEnemy enemy = hit.GetComponent<TestEnemy>();
                    if (enemy != null)
                    {
                        enemy.OnHit();
                    }
                }
            }

            // advance combo index (0,1,2) then wrap
            _attackComboNum++;

            // reset timers
            _attackDelta = meleeAttackCooldown * (1/baseMeleeAttackSpeed); // set cooldown before next attack allowed
            _attackComboDelta = comboResetTime * (1/baseMeleeAttackSpeed); // refresh combo window

            playerMovementController?.SetIsPerformingAction(false);
        }

        private void RangedAttack()
        {
            // Implement ranged attack logic here using baseRangedAttackRange, baseRangedAttackDamage, etc.
            // Example: Instantiate a projectile and set its speed and area of effect
        }

        private void Block()
        {
            // Implement block logic here using baseBlockEfficiency and baseParryTimingWindow
            // Example: Reduce incoming damage based on block efficiency and check for parry timing
        }

        private void OnDrawGizmos()
        {
            float attackRange = baseMeleeAttackRange;
            float attackAngle = 90f;
            Vector3 origin = transform.position + Vector3.up;
            Vector3 forward = transform.forward;

            if (_attackDelta > 0|| _attackComboDelta > 0)
            {
                //transparent red when on cooldown
                Handles.color = new Color(1f, 0f, 0f, 0.3f);
            }
            else
            {
                Handles.color = new Color(0f, 1f, 0f, 0.3f);
            }
            Handles.DrawSolidArc(origin, Vector3.up, Quaternion.Euler(0, -attackAngle/2, 0) * forward, attackAngle, attackRange);
        }

    }
}