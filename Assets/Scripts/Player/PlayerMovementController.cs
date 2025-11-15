using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    public class PlayerMovementController : Singleton<PlayerMovementController>
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;

        public float moveSpeedBonus = 0.0f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;

        [Tooltip("Dash speed of the character in m/s")]
        public float dashSpeed = 30.0f;

        [Tooltip("Dash distance of the character in meters")]
        public float dashDistance = 3.0f;

        [Space(10)] [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to dash again. Set to 0f to instantly dash")]
        public float dashTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool grounded = true;

        [Tooltip("Useful for rough ground")] public float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask groundLayers;

        //player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private readonly float _terminalVelocity = 53.0f;
        private bool _isDashing;
        private Vector3 _dashDirection;
        private float _dashTimeRemaining;
        private bool _isPerformingAction;

        // timeout delta-time
        private float _fallTimeoutDelta;
        private float _dashTimeoutDelta;

        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;

        private bool _hasAnimator; // kept only to avoid large refactor; not used for setting states now

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        private InputAction _moveAction;
        private InputAction _dashAction;


        void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            // cache actions once
            if (playerInput != null)
            {
                var map = playerInput.currentActionMap;
                _moveAction = map?.FindAction("Move") ?? playerInput.actions.FindAction("Move");
                _dashAction = map?.FindAction("Dash") ?? playerInput.actions.FindAction("Dash");
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator); // legacy, not used directly for state
            _controller = GetComponent<CharacterController>();

            // reset our timeouts on start
            _fallTimeoutDelta = fallTimeout;
        }

        // Update is called once per frame
        void Update()
        {
            GroundedCheck();
            ApplyGravity();
            Dash();
            Move();
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
                transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            PlayerAnimationController.Instance.SetGrounded(grounded);
        }

        private void Move()
        {
            if (_isDashing || _isPerformingAction)
            {
                return;
            }

            Vector2 moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = moveSpeed + (moveSpeed * (moveSpeedBonus/100));

            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error-prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    rotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            PlayerAnimationController.Instance.SetSpeed(_animationBlend, inputMagnitude);
        }

        private void ApplyGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer and exit free fall when grounded
                _fallTimeoutDelta = fallTimeout;
                PlayerAnimationController.Instance.SetFreeFall(false);

                // small downward force to keep grounded contact
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                // fall timeout while not grounded; after it elapses, set free fall
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    PlayerAnimationController.Instance.SetFreeFall(true);
                }
            }

            // apply gravity over time if under terminal velocity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void Dash()
        {
            // If currently dashing, handle the dash movement
            if (_isDashing)
            {
                // keep dash timer ticking
                if (_dashTimeRemaining > 0f)
                {
                    _dashTimeRemaining -= Time.deltaTime;

                    // Apply dash movement
                    _controller.Move(_dashDirection.normalized * (dashSpeed * Time.deltaTime) +
                                     new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                }
                else
                {
                    // Dash finished, start cooldown
                    _isDashing = false;
                    _dashTimeoutDelta = dashTimeout; // Start cooldown timer
                    PlayerAnimationController.Instance.SetDash(false);
                }

                PlayerAnimationController.Instance.SetDash(true); // ensure dash state during dash
                return;
            }

            // If not dashing, count down the cooldown timer
            if (_dashTimeoutDelta > 0.0f)
            {
                _dashTimeoutDelta -= Time.deltaTime;
            }

            // Check for dash input (only when not dashing and cooldown is complete)
            bool dashInput = _dashAction != null && _dashAction.triggered;

            if (dashInput && _dashTimeoutDelta <= 0.0f)
            {
                // choose dash direction based on current move input and camera, fallback to forward
                Vector2 moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

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

                _isDashing = true;
                _dashTimeRemaining = dashDistance / dashSpeed;

                PlayerAnimationController.Instance.SetDash(true);
            }
            else
            {
                PlayerAnimationController.Instance.SetDash(false);
            }
        }

        // remove animation event handlers & OnJump replaced by animationController.TriggerJumpFeedback
        public void SetPlayerRotation(Vector3 lookDirection)
        {
            if (lookDirection.sqrMagnitude < 0.001f)
                return;

            Vector3 flatDir = new Vector3(lookDirection.x, 0f, lookDirection.z);
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = targetRot;

        }
        public void SetIsPerformingAction(bool isPerforming) { _isPerformingAction = isPerforming; }

        public void ApplyMovementSpeedPowerup(float value)
        {
            moveSpeedBonus += value;
        }
    }
}
