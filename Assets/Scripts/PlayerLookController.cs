using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class PlayerLookController : MonoBehaviour
    {
        public Vector3 CurrentLookDirection { get; private set; }

        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private CinemachinePositionComposer cinemachinePositionComposer;

        [Header("Settings")]
        [SerializeField] private float lookMaxDistance = 5f;

        private void Awake()
        {
            if (playerBody == null)
            {
                Debug.LogWarning("Player Body Transform not assigned in player look controller.");
                playerBody = transform;
            }
            if (cameraTransform == null)
            {
                Debug.LogWarning("Camera Transform not assigned in player look controller.");
                cameraTransform = Camera.main.transform;
            }
            if (cinemachinePositionComposer == null)
            {
                Debug.LogWarning("Cinemachine Position Composer not assigned in player look controller.");
                cinemachinePositionComposer = FindFirstObjectByType<CinemachinePositionComposer>();
            }
        }

        private void Update()
        {
            Look();
        }

        private void Look()
        {
            Vector2 lookInput = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();

            //TODO: make alternate method for controller input

            // Convert mouse look (screen-space) input to a (world-space) direction vector
            float normalizedX = (lookInput.x / Screen.width - 0.5f) * 2f;
            float normalizedY = (lookInput.y / Screen.height - 0.5f) * 2f;
            Vector3 lookDirection = new Vector3(normalizedX, 0f, normalizedY);
            Debug.Log(lookDirection);

            // ok so this took a while
            // the position composer is based on the player transform so need to rotate the look direction to prevent its axis from rotating with the player
            // when we do this, it stops rotating with the player, but it moves along with world-space x-y axis
            // so now we need to rotate it based on the camera's rotation relative to the player
            // but we can't do two rotations sequentially, we have to calculate the needed rotation based on both transforms
            float rotationDelta = Camera.main.transform.eulerAngles.y - playerBody.eulerAngles.y;
            Quaternion relativeRotation = Quaternion.Euler(0f, rotationDelta, 0f);
            Vector3 worldLookDirection = relativeRotation * lookDirection;

            // Set look magnitude based on distance from screen center
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            float distanceFromCenter = (lookInput - screenCenter).magnitude;
            float maxPossibleDistance = screenCenter.magnitude;
            float normalizedMaxDistance = Mathf.Clamp01(distanceFromCenter / maxPossibleDistance) * lookMaxDistance;

            cinemachinePositionComposer.TargetOffset = worldLookDirection * normalizedMaxDistance;
        }
    }
}