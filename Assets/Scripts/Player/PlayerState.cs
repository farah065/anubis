using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    /// <summary>
    /// Abstract base class for all player states.
    /// Each state encapsulates behavior, transitions, and data for that specific state.
    /// </summary>
    public abstract class PlayerState
    {
        /// <summary>
        /// Called once when entering this state.
        /// Use for: setting animations, locking movement, resetting timers.
        /// </summary>
        public virtual void Enter(PlayerStateMachine player)
        {
        }

        /// <summary>
        /// Called once when exiting this state.
        /// Use for: cleanup, unlocking movement, storing cooldowns.
        /// </summary>
        public virtual void Exit(PlayerStateMachine player) { }

        /// <summary>
        /// Called every frame while in this state.
        /// Use for: ticking timers, checking duration-based transitions.
        /// </summary>
        public virtual void Update(PlayerStateMachine player) { }

        /// <summary>
        /// Called when input is received while in this state.
        /// Return a new state to transition, or null to stay in current state.
        /// </summary>
        public virtual PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            return null;
        }

        /// <summary>
        /// Whether this state allows movement input.
        /// </summary>
        public virtual bool AllowsMovement() => true;

        /// <summary>
        /// Whether this state can be interrupted by other actions.
        /// </summary>
        public virtual bool CanBeInterrupted() => true;
    }

#region Idle State

    public class IdleState : PlayerState
    {
        public static readonly IdleState Instance = new IdleState();

        public override void Enter(PlayerStateMachine player)
        {
            PlayerAnimationController.Instance.SetMelee(-1); // Reset melee parameter
            PlayerAnimationController.Instance.SetSpeed(0f, 0f);
            player.SetIsPerformingAction(false);
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // Can transition to any action from idle
            if (action.name == "Melee Attack" && player.CanMeleeAttack())
            {
                Debug.Log("Input to MeleeAttack1State");
                return MeleeAttack0State.Instance;
            }

            if (action.name == "Ranged Attack" && player.CanRangedAttack())
            {
                return new RangedAttackState();
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            if (action.name == "Dash" && player.CanDash())
            {
                return DashState.Instance;
            }

            return null;
        }
    }

#endregion

#region Moving State

    public class MovingState : PlayerState
    {
        public static readonly MovingState Instance = new MovingState();

        public override void Update(PlayerStateMachine player)
        {
            // If movement stops, return to idle
            if (player.GetMoveInput().magnitude < 0.01f)
            {
                player.ChangeState(IdleState.Instance);
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // Same transitions as idle - all actions can cancel movement
            if (action.name == "Melee Attack" && player.CanMeleeAttack())
            {
                return MeleeAttack0State.Instance;
            }

            if (action.name == "Ranged Attack" && player.CanRangedAttack())
            {
                return new RangedAttackState();
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            if (action.name == "Dash" && player.CanDash())
            {
                return DashState.Instance;
            }

            return null;
        }
    }

#endregion

#region Dash State

    public class DashState : PlayerState
    {
        public static readonly DashState Instance = new DashState();

        private float _dashTimeRemaining;

        public override void Enter(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(true);
            PlayerAnimationController.Instance.SetDash(true);

            // Initialize dash
            _dashTimeRemaining = player.DashDistance / player.DashSpeed;
            player.InitializeDash();
        }

        public override void Exit(PlayerStateMachine player)
        {
            PlayerAnimationController.Instance.SetDash(false);
            player.SetIsPerformingAction(false);
            player.StartDashCooldown();
        }

        public override void Update(PlayerStateMachine player)
        {
            if (_dashTimeRemaining > 0f)
            {
                _dashTimeRemaining -= Time.deltaTime;
                player.ApplyDashMovement();
            }
            else
            {
                // Dash finished, return to idle/moving based on input
                if (player.GetMoveInput().magnitude > 0.01f)
                {
                    player.ChangeState(MovingState.Instance);
                }
                else
                {
                    player.ChangeState(IdleState.Instance);
                }
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // Dash can be canceled by any action
            if (action.name == "Melee Attack" && player.CanMeleeAttack())
            {
                return MeleeAttack0State.Instance;
            }

            if (action.name == "Ranged Attack" && player.CanRangedAttack())
            {
                return new RangedAttackState();
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            return null;
        }

        public override bool AllowsMovement() => false;
    }

#endregion

#region Melee Attack States (Combo Chain)

    public class MeleeAttack0State : PlayerState
    {
        public static readonly MeleeAttack0State Instance = new MeleeAttack0State();

        private bool _nextAttackQueued = false;
        private float _lastInputTime = 0f;
        private const float COMBO_WINDOW = 4f; // Max time to continue combo

        public override void Enter(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(true);
            player.StartCoroutine(player.PerformMeleeAttack(0));

            _nextAttackQueued = false;
            _lastInputTime = Time.time;

            player.EnableMeleeAttackHitbox(true);
        }

        public override void Exit(PlayerStateMachine player)
        {
            player.EnableMeleeAttackHitbox(false);
            PlayerAnimationController.Instance.SetMelee(-1); // Reset melee parameter
        }

        public override void Update(PlayerStateMachine player)
        {
            // Combo timeout
            if (Time.time - _lastInputTime > COMBO_WINDOW)
            {
                player.ChangeState(IdleState.Instance);
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            if (action.name == "Melee Attack")
            {
                _nextAttackQueued = true;
                _lastInputTime = Time.time;
            }

            // Other actions can cancel attack
            if (action.name == "Ranged Attack" && player.CanRangedAttack())
            {
                return new RangedAttackState();
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            if (action.name == "Dash" && player.CanDash())
            {
                return DashState.Instance;
            }

            return null;
        }

        /// <summary>
        /// Called by animation event to check if combo should continue.
        /// </summary>
        public PlayerState TryContinueCombo(PlayerStateMachine player)
        {
            if (_nextAttackQueued && Time.time - _lastInputTime <= COMBO_WINDOW)
            {
                Debug.Log("Continuing to MeleeAttack1State");
                return MeleeAttack1State.Instance;
            }
            return IdleState.Instance;
        }

        public override bool AllowsMovement() => false;
    }

    public class MeleeAttack1State : PlayerState
    {
        public static readonly MeleeAttack1State Instance = new MeleeAttack1State();

        private bool _nextAttackQueued = false;
        private float _lastInputTime = 0f;
        private const float COMBO_WINDOW = 4f;

        public override void Enter(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(true);
            player.StartCoroutine(player.PerformMeleeAttack(1));

            _nextAttackQueued = false;
            _lastInputTime = Time.time;

            player.EnableMeleeAttackHitbox(true);
        }

        public override void Exit(PlayerStateMachine player)
        {
            player.EnableMeleeAttackHitbox(false);
            PlayerAnimationController.Instance.SetMelee(-1); // Reset melee parameter
        }

        public override void Update(PlayerStateMachine player)
        {
            if (Time.time - _lastInputTime > COMBO_WINDOW)
            {
                player.ChangeState(IdleState.Instance);
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            if (action.name == "Melee Attack")
            {
                _nextAttackQueued = true;
                _lastInputTime = Time.time;
            }

            if (action.name == "Ranged Attack" && player.CanRangedAttack())
            {
                return new RangedAttackState();
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            if (action.name == "Dash" && player.CanDash())
            {
                return DashState.Instance;
            }

            return null;
        }

        public PlayerState TryContinueCombo(PlayerStateMachine player)
        {
            if (_nextAttackQueued && Time.time - _lastInputTime <= COMBO_WINDOW)
            {
                Debug.Log("Continuing to MeleeAttack1State");
                return MeleeAttack2State.Instance;
            }
            return IdleState.Instance;
        }

        public override bool AllowsMovement() => false;
    }

    public class MeleeAttack2State : PlayerState
    {
        public static readonly MeleeAttack2State Instance = new MeleeAttack2State();

        private float _lastInputTime = 0f;
        private const float COMBO_WINDOW = 4f;

        public override void Enter(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(true);
            player.StartCoroutine(player.PerformMeleeAttack(2));

            _lastInputTime = Time.time;

            player.EnableMeleeAttackHitbox(true);
        }

        public override void Exit(PlayerStateMachine player)
        {
            player.EnableMeleeAttackHitbox(false);
            PlayerAnimationController.Instance.SetMelee(-1); // Reset melee parameter
            player.StartMeleeCooldown();
        }

        public override void Update(PlayerStateMachine player)
        {
            if (Time.time - _lastInputTime > COMBO_WINDOW)
            {
                player.ChangeState(IdleState.Instance);
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // Final attack in combo - can still be canceled
            if (action.name == "Ranged Attack" && player.CanRangedAttack())
            {
                return new RangedAttackState();
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            if (action.name == "Dash" && player.CanDash())
            {
                return DashState.Instance;
            }

            return null;
        }

        public PlayerState FinishCombo(PlayerStateMachine player)
        {
            return IdleState.Instance;
        }

        public override bool AllowsMovement() => false;
    }

#endregion

#region Ranged Attack State

    public class RangedAttackState : PlayerState
    {
        private float _attackDuration = 0.5f; // Duration of ranged attack animation
        private float _timer = 0f;
        private bool _projectileSpawned = false;

        public override void Enter(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(true);
            player.SetPlayerRotation(PlayerLookController.Instance.CurrentAimDirection);

            _timer = 0f;
            _projectileSpawned = false;

            // TODO: Set ranged attack animation
        }

        public override void Exit(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(false);
            player.StartRangedCooldown();
        }

        public override void Update(PlayerStateMachine player)
        {
            _timer += Time.deltaTime;

            // Spawn projectile at peak of animation (halfway through)
            if (!_projectileSpawned && _timer >= _attackDuration * 0.5f)
            {
                player.SpawnProjectile();
                _projectileSpawned = true;
            }

            // End attack after animation completes
            if (_timer >= _attackDuration)
            {
                if (player.GetMoveInput().magnitude > 0.01f)
                {
                    player.ChangeState(MovingState.Instance);
                }
                else
                {
                    player.ChangeState(IdleState.Instance);
                }
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // Ranged attack can be canceled
            if (action.name == "Melee Attack" && player.CanMeleeAttack())
            {
                return MeleeAttack0State.Instance;
            }

            if (action.name == "Block" && action.IsPressed())
            {
                return new BlockingState();
            }

            if (action.name == "Dash" && player.CanDash())
            {
                return DashState.Instance;
            }

            return null;
        }

        public override bool AllowsMovement() => false;
    }

#endregion

#region Blocking State

    public class BlockingState : PlayerState
    {
        private bool _isInParryWindow = false;
        private float _parryWindowTimer = 0f;
        private const float PARRY_WINDOW_DURATION = 0.3f;

        public bool IsInParryWindow => _isInParryWindow;

        public override void Enter(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(true);
            PlayerAnimationController.Instance.SetBlock(true);

            // Open parry window at start of block (if cooldown allows)
            if (player.CanParry())
            {
                _isInParryWindow = true;
                _parryWindowTimer = PARRY_WINDOW_DURATION;
                player.StartParryCooldown();
            }
        }

        public override void Exit(PlayerStateMachine player)
        {
            player.SetIsPerformingAction(false);
            PlayerAnimationController.Instance.SetBlock(false);
            _isInParryWindow = false;
        }

        public override void Update(PlayerStateMachine player)
        {
            // Tick parry window
            if (_isInParryWindow)
            {
                _parryWindowTimer -= Time.deltaTime;
                if (_parryWindowTimer <= 0f)
                {
                    _isInParryWindow = false;
                }
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // Block released - return to previous state
            if (action.name == "Block" && !action.IsPressed())
            {
                if (player.GetMoveInput().magnitude > 0.01f)
                {
                    return MovingState.Instance;
                }
                return IdleState.Instance;
            }

            // Blocking prevents all other actions
            return null;
        }

        public override bool AllowsMovement() => false;
        public override bool CanBeInterrupted() => false; // Blocking can't be canceled except by releasing block
    }

#endregion

#region Airborne State

    public class AirborneState : PlayerState
    {
        public static readonly AirborneState Instance = new AirborneState();

        public override void Enter(PlayerStateMachine player)
        {
            PlayerAnimationController.Instance.SetFreeFall(true);
        }

        public override void Exit(PlayerStateMachine player)
        {
            PlayerAnimationController.Instance.SetFreeFall(false);
        }

        public override void Update(PlayerStateMachine player)
        {
            // Check if landed
            if (player.IsGrounded())
            {
                if (player.GetMoveInput().magnitude > 0.01f)
                {
                    player.ChangeState(MovingState.Instance);
                }
                else
                {
                    player.ChangeState(IdleState.Instance);
                }
            }
        }

        public override PlayerState HandleInput(PlayerStateMachine player, InputAction action)
        {
            // No actions allowed while airborne (for now)
            return null;
        }

        public override bool AllowsMovement() => true; // Can still control aerial movement
    }

#endregion
}
