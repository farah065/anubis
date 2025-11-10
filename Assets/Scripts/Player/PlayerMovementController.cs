using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;

        [Tooltip("Dash speed of the character in m/s")]
        public float dashSpeed = 30.0f;

        [Tooltip("Dash distance of the character in meters")]
        public float dashDistance = 3.0f;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float jumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.50f;

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

        [Header("Feedbacks")] // remove individual feedbacks now handled by animation controller
        [SerializeField] private PlayerAnimationController animationController;

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
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _dashTimeoutDelta;

        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;

        private bool _hasAnimator; // kept only to avoid large refactor; not used for setting states now

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;


        void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator); // legacy, not used directly for state
            _controller = GetComponent<CharacterController>();

            // reset our timeouts on start
            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        // Update is called once per frame
        void Update()
        {
            JumpAndGravity();
            GroundedCheck();
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

            animationController?.SetGrounded(grounded);
        }

        private void Move()
        {
            if (_isDashing || _isPerformingAction)
            {
                return;
            }

            var moveAction = playerInput.actions.FindAction("Move");
            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = moveSpeed;

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

            animationController?.SetSpeed(_animationBlend, inputMagnitude);
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                animationController?.SetJump(false);
                animationController?.SetFreeFall(false);

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                var jumpAction = playerInput.actions.FindAction("Jump");
                bool jumpInput = jumpAction != null && jumpAction.triggered;

                if (jumpInput && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    animationController?.SetJump(true);
                    animationController?.TriggerJumpFeedback();
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    animationController?.SetFreeFall(true);
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
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
                    animationController?.SetDash(false);
                }

                animationController?.SetDash(true); // ensure dash state during dash
                return;
            }

            // If not dashing, count down the cooldown timer
            if (_dashTimeoutDelta > 0.0f)
            {
                _dashTimeoutDelta -= Time.deltaTime;
            }

            // Check for dash input (only when not dashing and cooldown is complete)
            var dashAction = playerInput.actions.FindAction("Dash");
            bool dashInput = dashAction != null && dashAction.triggered;

            if (dashInput && _dashTimeoutDelta <= 0.0f)
            {
                // choose dash direction based on current move input and camera, fallback to forward
                var moveAction = playerInput.actions.FindAction("Move");
                Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

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

                animationController?.SetDash(true);
            }
            else
            {
                animationController?.SetDash(false);
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
    }
}
