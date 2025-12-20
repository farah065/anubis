using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    /// <summary>
    /// Main player controller using State Pattern.
    /// Manages current state, handles input delegation, and provides state access to player data.
    /// </summary>
    public class PlayerStateMachine : Singleton<PlayerStateMachine>
    {
        [Header("References")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject meleeAttackHitbox;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController controller;
        [SerializeField] public GameObject Trail;
         
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float dashSpeed = 30.0f;
        [SerializeField] private float dashDistance = 3.0f;
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float speedChangeRate = 10.0f;

        [Header("Combat Settings")]
        [SerializeField] private float baseMeleeAttackDamage = 10f;
        [SerializeField] private float baseMeleeAttackKnockback = 5f;
        [SerializeField] private float meleeAttackLungeDistance = 0.5f;
        [SerializeField] private float baseRangedAttackDamage = 8f;
        [SerializeField] private float baseRangedAttackKnockback = 1f;
        [SerializeField] private float baseRangedAttackSpeed = 1.5f;
        [SerializeField] private float baseRangedAttackRange = 10f;
        [SerializeField] private float baseRangedAttackArea = 0f;

        [Header("Cooldown Settings")]
        [SerializeField] private float meleeCooldownDuration = 2f;
        [SerializeField] private float rangedCooldownDuration = 5f;
        [SerializeField] private float dashCooldownDuration = 0.5f;
        [SerializeField] private float parryCooldownDuration = 1f;

        [Header("Grounded Check")]
        [SerializeField] private float groundedOffset = -0.14f;
        [SerializeField] private float groundedRadius = 0.28f;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float gravity = -15.0f;

        // Power-up bonuses
        public float MeleeAttackDamageBonus = 0.0f;
        public float MeleeAttackKnockbackBonus = 0.0f;
        public float RangedAttackDamageBonus = 0.0f;
        public float RangedAttackKnockbackBonus = 0.0f;
        public float MoveSpeedBonus = 0.0f;

        // State machine
        private PlayerState _currentState;

        // Cooldown timers
        private float _meleeCooldownTimer = 0f;
        private float _rangedCooldownTimer = 0f;
        private float _dashCooldownTimer = 0f;
        private float _parryCooldownTimer = 0f;

        // Movement data
        private Vector3 _dashDirection;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private readonly float _terminalVelocity = 53.0f;
        private bool _isPerformingAction = false;
        private bool _grounded = true;

        // Cached references
        private GameObject _mainCamera;
        private InputAction _moveAction;
        private InputAction _meleeAction;
        private InputAction _rangedAction;
        private InputAction _blockAction;
        private InputAction _dashAction;

        // Attack data
        public AttackData MeleeAttackData { get; private set; }

        // Public accessors for states
        public float DashSpeed => dashSpeed;
        public float DashDistance => dashDistance;

        void Start()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            // Cache input actions
            if (playerInput != null)
            {
                var map = playerInput.currentActionMap;
                _moveAction = map?.FindAction("Move") ?? playerInput.actions.FindAction("Move");
                _meleeAction = map?.FindAction("MeleeAttack") ?? playerInput.actions.FindAction("Melee Attack");
                _rangedAction = map?.FindAction("RangedAttack") ?? playerInput.actions.FindAction("Ranged Attack");
                _blockAction = map?.FindAction("Block") ?? playerInput.actions.FindAction("Block Hold");
                _dashAction = map?.FindAction("Dash") ?? playerInput.actions.FindAction("Dash");
            }

            InitializeMeleeAttackData();

            // Start in idle state
            _currentState = IdleState.Instance;
            _currentState.Enter(this);
        }

        void Update()
        {
            UpdateCooldowns();
            GroundedCheck();
            ApplyGravity();

            // Update current state
            _currentState.Update(this);

            // Handle movement if allowed
            if (_currentState.AllowsMovement())
            {
                HandleMovement();
            }

            // Handle input
            HandleInput();

            // Check for state transition to airborne
            if (!_grounded && !(_currentState is AirborneState))
            {
                ChangeState(AirborneState.Instance);
            }
        }

        #region State Management

        public PlayerState CurrentState => _currentState;

        public void ChangeState(PlayerState newState)
        {
            if (newState == null) return;

            _currentState?.Exit(this);
            _currentState = newState;
            _currentState.Enter(this);
        }

        private void HandleInput()
        {
            if (_meleeAction != null && _meleeAction.WasPerformedThisFrame())
            {
                var newState = _currentState.HandleInput(this, _meleeAction);
                if (newState != null) ChangeState(newState);
            }

            if (_rangedAction != null && _rangedAction.WasPerformedThisFrame())
            {
                var newState = _currentState.HandleInput(this, _rangedAction);
                if (newState != null) ChangeState(newState);
            }

            if (_blockAction != null)
            {
                var newState = _currentState.HandleInput(this, _blockAction);
                if (newState != null) ChangeState(newState);
            }

            if (_dashAction != null && _dashAction.WasPerformedThisFrame())
            {
                var newState = _currentState.HandleInput(this, _dashAction);
                if (newState != null) ChangeState(newState);
            }
        }

        #endregion

        #region Cooldown Management

        private void UpdateCooldowns()
        {
            if (_meleeCooldownTimer > 0f) _meleeCooldownTimer -= Time.deltaTime;
            if (_rangedCooldownTimer > 0f) _rangedCooldownTimer -= Time.deltaTime;
            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;
            if (_parryCooldownTimer > 0f) _parryCooldownTimer -= Time.deltaTime;
        }

        public bool CanMeleeAttack() => _meleeCooldownTimer <= 0f;
        public bool CanRangedAttack() => _rangedCooldownTimer <= 0f;
        public bool CanDash() => _dashCooldownTimer <= 0f;
        public bool CanParry() => _parryCooldownTimer <= 0f;

        public void StartMeleeCooldown() => _meleeCooldownTimer = meleeCooldownDuration;
        public void StartRangedCooldown() => _rangedCooldownTimer = rangedCooldownDuration;
        public void StartDashCooldown() => _dashCooldownTimer = dashCooldownDuration;
        public void StartParryCooldown() => _parryCooldownTimer = parryCooldownDuration;
        // Immediately reset the parry cooldown timer (set to 0)
        public void ResetParryCooldown() => _parryCooldownTimer = 0f;

        #endregion

        #region Movement

        private void HandleMovement()
        {
            Vector2 moveInput = GetMoveInput();

            // Check for movement state transition
            if (moveInput.magnitude > 0.01f && _currentState is IdleState)
            {
                ChangeState(MovingState.Instance);
            }

            float targetSpeed = moveSpeed + (moveSpeed * (MoveSpeedBonus / 100));
            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                    ref _rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                           new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            PlayerAnimationController.Instance.SetSpeed(_animationBlend, inputMagnitude);
        }

        public void InitializeDash()
        {
            Vector2 moveInput = GetMoveInput();

            if (moveInput != Vector2.zero)
            {
                Vector3 inputDir = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
                float targetRot = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                _dashDirection = Quaternion.Euler(0.0f, targetRot, 0.0f) * Vector3.forward;
            }
            else
            {
                _dashDirection = transform.forward;
            }
        }

        public void ApplyDashMovement()
        {
            controller.Move(_dashDirection.normalized * (dashSpeed * Time.deltaTime) +
                           new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        public Vector2 GetMoveInput()
        {
            return _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        }

        public void SetPlayerRotation(Vector3 lookDirection)
        {
            if (lookDirection.sqrMagnitude < 0.001f) return;
            Vector3 flatDir = new Vector3(lookDirection.x, 0f, lookDirection.z);
            transform.rotation = Quaternion.LookRotation(flatDir);
        }

        public void SetIsPerformingAction(bool isPerforming)
        {
            _isPerformingAction = isPerforming;
        }

        #endregion

        #region Grounded & Gravity

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x,
                transform.position.y - groundedOffset, transform.position.z);
            _grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            PlayerAnimationController.Instance.SetGrounded(_grounded);
        }

        private void ApplyGravity()
        {
            if (_grounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        public bool IsGrounded() => _grounded;

        #endregion

        #region Combat

        private void InitializeMeleeAttackData()
        {
            float damage = baseMeleeAttackDamage + (baseMeleeAttackDamage * (MeleeAttackDamageBonus / 100));
            float knockback = baseMeleeAttackKnockback + (baseMeleeAttackKnockback * (MeleeAttackKnockbackBonus / 100));
            MeleeAttackData = new AttackData();
            MeleeAttackData.Initialize(damage, knockback);
        }
        public void ApplyMeleeAttackLunge(Vector3 dir)
        {
            Debug.Log("Applying melee attack lunge.");
            // Move player forward in the direction they're facing
            controller.Move(dir * meleeAttackLungeDistance);
        }

        
        public IEnumerator PerformMeleeAttack(int stage)
        {
            SetPlayerRotation(PlayerLookController.Instance.CurrentAimDirection);
            //ApplyMeleeAttackLunge(transform.forward); fuck this thing
            PlayerAnimationController.Instance.SetMelee(stage);
            yield return null;
        }

        public void SpawnProjectile()
        {
            if (projectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up + transform.forward * 0.5f;
                Quaternion spawnRot = Quaternion.LookRotation(transform.forward, Vector3.up);
                var go = Instantiate(projectilePrefab, spawnPos, spawnRot);
                var proj = go.GetComponent<PlayerProjectile>();

                float damage = baseRangedAttackDamage + (baseRangedAttackDamage * (RangedAttackDamageBonus / 100));
                float knockback = baseRangedAttackKnockback + (baseRangedAttackKnockback * (RangedAttackKnockbackBonus / 100));

                if (proj != null)
                {
                    proj.Initialize(damage, knockback, baseRangedAttackSpeed, baseRangedAttackRange, baseRangedAttackArea);
                }
            }
        }

        #endregion

        #region Animation Events

        public void EnableMeleeAttackHitbox()
        {
            meleeAttackHitbox.SetActive(true);
        }

        public void DisableMeleeAttackHitbox()
        {
            meleeAttackHitbox.SetActive(false);
        }        


        public void OnMeleeAttackComplete()
        {
            DisableMeleeAttackHitbox();
            // Called at end of each attack animation
            if (_currentState is MeleeAttack0State attack1)
            {
                var nextState = attack1.TryContinueCombo(this);
                ChangeState(nextState);
            }
            else if (_currentState is MeleeAttack1State attack2)
            {
                var nextState = attack2.TryContinueCombo(this);
                ChangeState(nextState);
            }
            else if (_currentState is MeleeAttack2State attack3)
            {
                var nextState = attack3.FinishCombo(this);
                ChangeState(nextState);
            }
        }

        public void EnableTrail()
        {
            if (Trail != null)
            {
                Trail.SetActive(true);
            }
        }

        public void DisableTrail()
        {
            if (Trail != null)
            {
                Trail.SetActive(false);
            }
        }

        #endregion

        #region Power-Ups

        public void ApplyMeleeAttackDamagePowerup(float value)
        {
            MeleeAttackDamageBonus += value;
            InitializeMeleeAttackData();
        }

        public void ApplyMeleeAttackKnockbackPowerup(float value)
        {
            MeleeAttackKnockbackBonus += value;
            InitializeMeleeAttackData();
        }

        public void ApplyRangedAttackDamagePowerup(float value)
        {
            RangedAttackDamageBonus += value;
        }

        public void ApplyRangedAttackKnockbackPowerup(float value)
        {
            RangedAttackKnockbackBonus += value;
        }

        public void ApplyMovementSpeedPowerup(float value)
        {
            MoveSpeedBonus += value;
        }

        public void ResetPowerups()
        {
            MeleeAttackDamageBonus = 0.0f;
            MeleeAttackKnockbackBonus = 0.0f;
            RangedAttackDamageBonus = 0.0f;
            RangedAttackKnockbackBonus = 0.0f;
            MoveSpeedBonus = 0.0f;
            InitializeMeleeAttackData();
        }
        #endregion

        #region Debug

        public string GetCurrentStateName()
        {
            return _currentState?.GetType().Name ?? "None";
        }

        // Added OnGUI to show timers and current state for quick debugging.
        private void OnGUI()
        {
            // Small UI in top-left corner
            GUILayout.BeginArea(new Rect(10, 10, 320, 220), GUI.skin.box);
            GUILayout.BeginVertical();

            GUILayout.Label($"State: {GetCurrentStateName()}");

            GUILayout.Label($"Melee CD: {Mathf.Max(0f, _meleeCooldownTimer):0.00}s");
            GUILayout.Label($"Ranged CD: {Mathf.Max(0f, _rangedCooldownTimer):0.00}s");
            GUILayout.Label($"Dash CD: {Mathf.Max(0f, _dashCooldownTimer):0.00}s");
            GUILayout.Label($"Parry CD: {Mathf.Max(0f, _parryCooldownTimer):0.00}s");

            // If currently blocking, show parry window state (BlockingState exposes IsInParryWindow)
            if (_currentState is BlockingState blockingState)
            {
                GUILayout.Label("Blocking: True");
                GUILayout.Label($"In Parry Window: {blockingState.IsInParryWindow}");
            }
            else
            {
                GUILayout.Label("Blocking: False");
            }

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                var currentClip = animator.GetCurrentAnimatorClipInfo(0);
                if (currentClip.Length > 0)
                {
                    GUILayout.Label($"Anim Clip: {currentClip[0].clip.name}");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}