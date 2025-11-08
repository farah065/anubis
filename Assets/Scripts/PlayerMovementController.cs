using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float moveSpeed = 2.0f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float speedChangeRate = 10.0f;

    [Tooltip("Dash speed of the character in m/s")]
    public float dashSpeed = 30.0f;

    [Tooltip("Dash distance of the character in meters")]
    public float dashDistance = 3.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
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

    [Tooltip("Useful for rough ground")]
    public float groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float groundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;

    [Header("Feedbacks")]
    public MMF_Player dashFeedbacks;
    public MMF_Player footstepFeedbacks;
    public MMF_Player jumpFeedbacks;
    public MMF_Player landFeedbacks;

    //player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation ;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private readonly float _terminalVelocity = 53.0f;
    private bool _isDashing;
    private Vector3 _dashDirection;
    private float _dashTimeRemaining;

    // timeout delta-time
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _dashTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDDash;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private bool _hasAnimator;


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
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();

        AssignAnimationIDs();

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

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDDash = Animator.StringToHash("Dash");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
            transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, grounded);
        }
    }

    private void Move()
    {
        if (_isDashing) { return; }
        Vector2 moveInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = moveSpeed;

        if(moveInput == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = moveInput.magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
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

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = grounded ? transparentGreen : transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            groundedRadius);
    }
    private void JumpAndGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                bool jumpInput = false;
                var jumpAction = InputSystem.actions.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpInput = jumpAction.triggered;
                }

                if (jumpInput && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }

                    OnJump();
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
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
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
                dashFeedbacks?.StopFeedbacks();
            }

            // Update animator and return early
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDDash, true);
            }
            return;
        }

        // If not dashing, count down the cooldown timer
        if (_dashTimeoutDelta > 0.0f)
        {
            _dashTimeoutDelta -= Time.deltaTime;
        }

        // Check for dash input (only when not dashing and cooldown is complete)
        bool dashInput = false;
        var dashAction = InputSystem.actions.FindAction("Dash");
        if (dashAction != null)
        {
            dashInput = dashAction.triggered;
        }

        if (dashInput && _dashTimeoutDelta <= 0.0f)
        {
            // choose dash direction based on current move input and camera, fallback to forward
            Vector2 moveInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
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

            dashFeedbacks?.PlayFeedbacks();
            // Update animator
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDDash, true);
            }
        }
        else if (_hasAnimator)
        {
            // Only set to false when not dashing
            _animator.SetBool(_animIDDash, false);
        }
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        footstepFeedbacks?.PlayFeedbacks();
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        landFeedbacks?.PlayFeedbacks();
    }

    private void OnJump()
    {
        jumpFeedbacks?.PlayFeedbacks();
    }
}
