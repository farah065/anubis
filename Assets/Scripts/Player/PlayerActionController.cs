﻿using System;
using Unity.Mathematics;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

#endif
using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    public class PlayerActionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovementController playerMovementController;
        [SerializeField] private PlayerLookController playerLookController;
        [SerializeField] private PlayerAnimationController playerAnimationController;

        [SerializeField] private GameObject projectilePrefab; // for ranged attacks
        [SerializeField] private Animator anim;
        [SerializeField] private AnimatorStateInfo _state;
        [SerializeField] private GameObject axeHitbox;


        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;

        [Header("Melee Attack Settings")]
        [SerializeField] private float baseMeleeAttackRange = 2f;
        [SerializeField] private float baseMeleeAttackDamage = 10;
        [SerializeField] private float baseMeleeAttackSpeed = 1f;
        [SerializeField] private float baseMeleeAttackKnockback = 5f;
        public AttackData meleeAttackData;

        public float meleeAttackCooldownTime = 2f;
        private float _meleeAttackNextFireTme = 1f;
        public static int NoOfClicks = 0;
        float _lastClickedTime = 0f;
        float _maxComboDelay = 4f;
        int _clicks = 0;   // how many extra clicks during current attack (1 or 2)
        float _lastClickTime = 0f;
        private bool _isAttacking = false;


        [Header("Ranged Attack Settings")]
        [SerializeField] private float baseRangedAttackRange = 10f;
        [SerializeField] private float baseRangedAttackDamage = 8;
        [SerializeField] private float baseRangedAttackKnockback = 1f;
        [SerializeField] private float baseRangedAttackSpeed = 1.5f;
        [SerializeField] private float baseRangedAttackArea = 0f;
        [SerializeField] private float rangedAttackCooldown = 5f;
        public AttackData rangedAttackData;

        [Header("Block Settings")]
        [SerializeField] private float baseBlockEfficiency = 0.5f;
        [SerializeField] private float baseParryTimingWindow = 0.3f;
        [SerializeField] private float parryActivationCooldown = 1.0f; // cooldown to prevent spamming parry

        // timeout delta-time
        private float _attackDelta;       // per-attack cooldown timer
        private float _attackComboDelta;  // combo continuation window timer
        private int _attackComboNum;      // current combo index (0..2)
        private int _maxCombo = 3;
        private int _attackHeightOffset = 1;
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
            meleeAttackData.Initialize((int)baseMeleeAttackDamage, baseMeleeAttackKnockback);

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
            anim = playerAnimationController.Animator;
        }

        private void Update()
        {
            MeleeAttack();
            RangedAttack();
            Block();

        }


        private void MeleeAttack()
        {
            axeHitbox.SetActive(_isAttacking);
            if (Time.time - _lastClickedTime> _maxComboDelay)
            {
                _isAttacking = false;
                _clicks = 0;
                anim.SetInteger("AttackIndex", 0);
                anim.SetBool("ReturnToIdle", true);
                playerMovementController?.SetIsPerformingAction(false);
            }
            if (_meleeAction != null && _meleeAction.WasPerformedThisFrame())
            {
                OnMeleeClick();
            }


        }

        private void OnMeleeClick()
        {
            _lastClickedTime = Time.time;
            //Debug.Log($"is attacking? : {isAttacking}");

            if(!_isAttacking)
            {
                _clicks = 1;
                _isAttacking = true;
                playerMovementController?.SetIsPerformingAction(true);
                playerMovementController?.SetPlayerRotation(playerLookController.CurrentAimDirection);
                //animator params
                //Debug.Log("Setting attack0");
                anim.SetBool("ReturnToIdle", false);
                anim.SetInteger("AttackIndex", 1);
                anim.SetTrigger("Attack");
                return;
            }

            _clicks = Mathf.Clamp(_clicks + 1, 1, 3);
            //Debug.Log($"Queued click while attacking: clicks = {clicks}");

        }

        public void ContinueCombo()
        {
            //Debug.Log("ContinueCombo called. clicks=" + clicks + " lastClickDelta=" + (Time.time - lastClickTime).ToString("F2"));

            int currentStep = anim.GetInteger("AttackIndex");
            int targetStep = _clicks;

            if (targetStep> currentStep && (Time.time - _lastClickedTime)<= _maxComboDelay)
            {
                int nextStep = currentStep + 1;
                nextStep = Mathf.Clamp(nextStep, 1, 3);
                playerMovementController?.SetIsPerformingAction(true);
                playerMovementController?.SetPlayerRotation(playerLookController.CurrentAimDirection);
                //anim params
                anim.SetBool("ReturnToIdle", false);
                anim.SetInteger("AttackIndex", nextStep);
                anim.SetTrigger("Attack"); // plays next clip
                //Debug.Log($"Chaining to next attack: {nextStep}");
                return;
            }
            // combo finished
            _clicks = 0;
            _isAttacking = false;
            anim.SetInteger("AttackIndex", 0);
            anim.SetBool("ReturnToIdle", true);
            //Debug.Log("Combo Ended");
            playerMovementController?.SetIsPerformingAction(false);
        }

        private bool IsInStateOrTransition(string stateName)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            AnimatorTransitionInfo trans = anim.GetAnimatorTransitionInfo(0);
            return info.IsName(stateName) || trans.IsUserName(stateName);
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
                    proj.Initialize(baseRangedAttackDamage, baseMeleeAttackKnockback, baseRangedAttackSpeed, baseRangedAttackRange, baseRangedAttackArea);
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
    }
}