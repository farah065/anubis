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
        [SerializeField] private PlayerAnimationController playerAnimationController;

        [SerializeField] private GameObject projectilePrefab; // for ranged attacks

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
        [SerializeField] private float rangedAttackCooldown = 5f;

        [Header("Block Settings")]
        [SerializeField] private float baseBlockEfficiency = 0.5f;
        [SerializeField] private float baseParryTimingWindow = 0.3f;
        [SerializeField] private float parryActivationCooldown = 1.0f; // cooldown to prevent spamming parry

        // timeout delta-time (repurposed)
        private float _attackDelta;       // per-attack cooldown timer
        private float _attackComboDelta;  // combo continuation window timer
        private int _attackComboNum;      // current combo index (0..2)
        private int MAX_COMBO = 3;
        private int ATTACK_HEIGHT_OFFSET = 1;
        private float _rangedAttackDelta;

        // Block / Parry state
        private InputAction _blockAction;
        private InputAction _meleeAction;
        private InputAction _rangedAction;
        private bool _isBlocking;
        private bool _isInParryWindow;
        private float _parryWindowTimer;
        private float _parryActivationTimer;
        private bool _parryConsumedThisHold;
        private bool _wasBlocking;

        // read-only accessors
        public bool IsBlocking => _isBlocking;
        public bool IsInParryWindow => _isInParryWindow;

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

            // cache block action if possible
            if (playerInput != null && playerInput.currentActionMap != null)
            {
                var map = playerInput.currentActionMap;
                _blockAction = map.FindAction("Block") ?? map.FindAction("Block Hold");
                // cache melee and ranged actions as well
                _meleeAction = map.FindAction("Melee Attack") ?? map.FindAction("MeleeAttack");
                _rangedAction = map.FindAction("Ranged Attack") ?? map.FindAction("RangedAttack");
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

            // read cached melee action
            bool attackTriggered = _meleeAction != null && _meleeAction.WasPerformedThisFrame();

            // only proceed if input triggered and per-attack cooldown finished and combo not maxed out
            if (!attackTriggered || _attackDelta > 0f)
            {
                playerAnimationController.SetMelee(-1);
                return;
            }
            if (_attackComboDelta > 0f && _attackComboNum == MAX_COMBO) return;

            // execute attack
            playerMovementController?.SetIsPerformingAction(true);
            playerMovementController?.SetPlayerRotation(playerLookController.CurrentAimDirection);

            // base stats (placeholders for future modifiers per combo stage)
            float attackRange = baseMeleeAttackRange; // could vary by _attackComboNum
            float attackDamage = baseMeleeAttackDamage; // could scale with combo index
            float attackKnockback = baseMeleeAttackKnockback; // could scale with combo index
            float attackAngle = 90f; // frontal arc

            Vector3 forward = transform.forward;
            Vector3 attackOrigin = transform.position + Vector3.up * ATTACK_HEIGHT_OFFSET;
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

            // trigger animation with current combo stage
            int stage = Mathf.Clamp(_attackComboNum, 0, MAX_COMBO - 1);
            playerAnimationController?.SetMelee(stage);

            // reset timers
            _attackDelta = meleeAttackCooldown * (1/baseMeleeAttackSpeed); // set cooldown before next attack allowed
            _attackComboDelta = comboResetTime * (1/baseMeleeAttackSpeed); // refresh combo window

            playerMovementController?.SetIsPerformingAction(false);
        }

        private void RangedAttack()
        {
            if (_rangedAttackDelta > 0f) _rangedAttackDelta -= Time.deltaTime;
            bool rangedTriggered = _rangedAction != null && _rangedAction.WasPerformedThisFrame();
             if (!rangedTriggered || _rangedAttackDelta > 0f) return;

             //TODO: this is always slightly off due to projectile being 1f off the ground, need to find a fix
             playerMovementController?.SetPlayerRotation(playerLookController.CurrentAimDirection);

             // instantiate projectile if prefab assigned
             if (projectilePrefab != null)
             {
                 Vector3 spawnPos = (transform.position + Vector3.up + transform.forward * 0.5f);
                 Quaternion spawnRot = Quaternion.LookRotation(transform.forward, Vector3.up);
                 var go = Instantiate(projectilePrefab, spawnPos, spawnRot);
                 var proj = go.GetComponent<PlayerProjectile>();
                 if (proj != null)
                 {
                     proj.Initialize(baseRangedAttackSpeed, baseRangedAttackRange, baseRangedAttackArea, enemyLayerMask);
                 }
             }
             else
             {
                 Debug.LogWarning("Projectile prefab not assigned on PlayerActionController.");
             }

             _rangedAttackDelta = rangedAttackCooldown; // start cooldown
         }

        private void Block()
        {
            // tick parry activation cooldown always
            if (_parryActivationTimer > 0f) _parryActivationTimer -= Time.deltaTime;

            // read block hold state
            bool blockHeld = _blockAction != null ? _blockAction.IsPressed() : (playerInput != null && playerInput.actions.FindAction("Block")?.IsPressed() == true);

            // update parry window countdown if active
            if (_isInParryWindow)
            {
                _parryWindowTimer -= Time.deltaTime;
                if (_parryWindowTimer <= 0f)
                {
                    _isInParryWindow = false;
                    // do NOT reset _parryConsumedThisHold here; keep consumed until release
                }
            }

            // Detect rising edge: block just started this frame
            bool justStartedHolding = blockHeld && !_wasBlocking;

            if (justStartedHolding)
            {
                // begin blocking
                _isBlocking = true;
                playerMovementController?.SetIsPerformingAction(true);
                playerAnimationController?.SetBlock(true);
                // mark parry as potentially consumable this hold
                _parryConsumedThisHold = false;

                // Only open parry window at the start of the hold, if activation cooldown allows
                if (_parryActivationTimer <= 0f)
                {
                    _isInParryWindow = true;
                    _parryWindowTimer = baseParryTimingWindow;
                    _parryActivationTimer = parryActivationCooldown;
                    _parryConsumedThisHold = true;
                }
            }
            else if (blockHeld && _isBlocking)
            {
                // still holding: do nothing else (parry won't reopen while holding)
            }
            else if (!blockHeld && _wasBlocking)
            {
                // block was released this frame
                _isBlocking = false;
                playerMovementController?.SetIsPerformingAction(false);
                playerAnimationController?.SetBlock(false);
                // reset consumed flag so next new hold can attempt parry (subject to cooldown)
                _parryConsumedThisHold = false;
                _isInParryWindow = false;
                _parryWindowTimer = 0f;
            }

            _wasBlocking = blockHeld;
         }

        private void OnAttackAnimationEnded()
        {
            playerMovementController?.SetIsPerformingAction(false);
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

        private void OnGUI()
        {
            // Small debug overlay for testing states
            GUILayout.BeginArea(new Rect(10, 10, 220, 140), "Player Debug", GUI.skin.window);
            GUILayout.Label($"IsBlocking: {_isBlocking}");
            GUILayout.Label($"IsInParryWindow: {_isInParryWindow}");
            GUILayout.Label($"ParryActivationCooldown: {_parryActivationTimer:F2}");
            GUILayout.Label($"AttackCooldown: {_attackDelta:F2}");
            GUILayout.Label($"ComboIndex: {_attackComboNum}");
            GUILayout.Label($"RangedCooldown: {_rangedAttackDelta:F2}");
            GUILayout.EndArea();
        }
    }
}
