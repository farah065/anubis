using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GEM
{
    public class PlayerLookController : Singleton<PlayerLookController>
    {
        public Vector3 CurrentAimDirection { get; private set; }

        [Header("References")]
        [SerializeField] private CinemachinePositionComposer cinemachinePositionComposer;
        private Camera mainCamera;

        [Header("Settings")]
        [SerializeField] private float lookMaxDistance = 5f;

        private bool _usingGamepad = false;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;

        private void Awake()
        {
            mainCamera = playerInput.camera;

            if (mainCamera == null)
            {
                Debug.LogWarning("Camera Transform not assigned in Player Input.");
                mainCamera = Camera.main;
            }
            if (cinemachinePositionComposer == null)
            {
                Debug.LogWarning("Cinemachine Position Composer not assigned in player look controller.");
                cinemachinePositionComposer = FindFirstObjectByType<CinemachinePositionComposer>();
            }
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.onControlsChanged += OnControlsChanged;
                UpdateControlScheme();
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.onControlsChanged -= OnControlsChanged;
            }
        }

        private void OnControlsChanged(PlayerInput pi) => UpdateControlScheme();
        private void UpdateControlScheme()
        {
            if (playerInput == null) { _usingGamepad = false; return; }
            _usingGamepad = playerInput.currentControlScheme == "Gamepad";
        }

        private void Update()
        {
            Look();
        }

        private void Look()
        {
            if (playerInput == null) return;
            var map = playerInput.currentActionMap;
            if (map == null) return;
            var lookAction = map.FindAction("Look");
            if (lookAction == null || !lookAction.enabled) return;

            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            Vector3 lookDirection;
            float normalizedMaxDistance;

            if (_usingGamepad)
            {
                // Gamepad stick: direct vector scaled by magnitude
                lookDirection = new Vector3(lookInput.x, 0f, lookInput.y);
                normalizedMaxDistance = Mathf.Clamp01(lookInput.magnitude) * lookMaxDistance;
            }
            else
            {
                // Mouse position style input (expects 'Look' bound to Pointer Position). If instead using delta, replace logic.
                float normalizedX = (lookInput.x / Screen.width - 0.5f) * 2f;
                float normalizedY = (lookInput.y / Screen.height - 0.5f) * 2f;
                lookDirection = new Vector3(normalizedX, 0f, normalizedY);

                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                // Set magnitude based on distance from screen center
                float distanceFromCenter = (lookInput - screenCenter).magnitude;
                float maxPossibleDistance = screenCenter.magnitude;
                normalizedMaxDistance = Mathf.Clamp01(distanceFromCenter / maxPossibleDistance) * lookMaxDistance;
            }

            // remove camera tilt from look direction to pass to action controller
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f;
            Quaternion cameraRotation = Quaternion.LookRotation(cameraForward.normalized, Vector3.up);
            Vector3 aimDirection = lookDirection + new Vector3(0, 1, 0); // this is incorrect
            CurrentAimDirection = cameraRotation * aimDirection.normalized;

            // ok so this took a while
            // the position composer is based on the player transform so need to rotate the look direction to prevent its axis from rotating with the player
            // when we do this, it stops rotating with the player, but it moves along with world-space x-y axis
            // so now we need to rotate it based on the camera's rotation relative to the player
            // but we can't do two rotations sequentially, we have to calculate the needed rotation based on both transforms
            float rotationDelta = Mathf.DeltaAngle(transform.eulerAngles.y, mainCamera.transform.eulerAngles.y);
            Quaternion relativeRotation = Quaternion.Euler(0f, rotationDelta, 0f);
            Vector3 worldLookDirection = relativeRotation * lookDirection;


            cinemachinePositionComposer.TargetOffset = worldLookDirection * normalizedMaxDistance;
        }
    }
}