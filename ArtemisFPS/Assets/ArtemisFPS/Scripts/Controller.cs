using UnityEngine;
using Unity.Cinemachine;

namespace ArtemisFPS.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class Controller : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField] private float _walkSpeed = 3.5f;
        [SerializeField] private float _sprintSpeed = 8f;
        [SerializeField] private float _jumpHeight = 2f;

        [Space(15)]
        [SerializeField] private float _accelerationRate = 20f;
        [SerializeField] private float _decelerationTime = 0.3f;

        private bool IsSprinting => IsSprintInput && CurrentSpeed > 0.1f;
        private float MaxSpeed => IsSprintInput ? _sprintSpeed : _walkSpeed;

        private Vector3 _decelerationSmoothRef;


        [Header("Looking Parameters")]
        [SerializeField] private Vector2 _lookSensitivity = new Vector2(0.1f, 0.1f);
        [SerializeField] private float _pitchClamp = 85f;

        [SerializeField] private float _currentPitch;

        private float CurrentPitch
        {
            get => _currentPitch;
            set => _currentPitch = Mathf.Clamp(value, -_pitchClamp, _pitchClamp);
        }


        [Header("Camera Parameters")]
        [SerializeField] private float _defaultFOV = 90f;
        [SerializeField, Range(1f, 10f)] private float _cameraFOVSmoothing = 3f;

        private float SprintFOV => _defaultFOV * 1.1f;
        private float TargetFOV => IsSprintInput ? SprintFOV : _defaultFOV;


        [Header("Physics Parameters")]
        [SerializeField] private float _gravityMultiplier = 4f;
        [SerializeField] private float _verticalVelocity = 0f;

        private Vector3 CurrentVelocity { get; set; }
        private float CurrentSpeed { get; set; }

        private bool IsGrounded => _characterController.isGrounded;


        [Header("Input")]
        public Vector2 MoveInput;
        public Vector2 LookInput;
        public bool IsSprintInput;


        [Header("Components")]
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private CharacterController _characterController;


        #region Unity Methods

        private void OnValidate()
        {
            if (_characterController == null)
            {
                _characterController = GetComponent<CharacterController>();
            }
        }

        private void Update()
        {
            MoveUpdate();
            LookUpdate();
            CameraUpdate();
        }

        #endregion

        #region Controller Methods

        public void TryJump()
        {
            if (!IsGrounded)
            {
                return;
            }

            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y * _gravityMultiplier);
        }

        private void MoveUpdate()
        {
            Vector3 motion = GetMovementVector();
            UpdateVelocity(motion);

            Vector3 fullVelocity = GetFullVelocity();
            CollisionFlags collisionFlags = ApplyMovement(fullVelocity);

            HandleVerticalCollision(collisionFlags);

            CurrentSpeed = fullVelocity.magnitude;
        }

        private void LookUpdate()
        {
            Vector2 input = GetLookInput();

            LookUpDown(input);

            LookLeftRight(input);
        }

        private void CameraUpdate()
        {
            UpdateCameraFOV();
        }


        private Vector3 GetMovementVector()
        {
            Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
            motion.y = 0;
            return motion.normalized;
        }

        private void UpdateVelocity(Vector3 motion)
        {
            Vector3 targetVelocity = motion * MaxSpeed;

            if (motion.sqrMagnitude > 0.01f)
            {
                CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, targetVelocity, _accelerationRate * Time.deltaTime);
            }
            else
            {
                CurrentVelocity = Vector3.SmoothDamp(CurrentVelocity, targetVelocity, ref _decelerationSmoothRef, _decelerationTime);
            }
        }

        private Vector3 GetFullVelocity()
        {
            float groundedStickVelocity = -3f;

            if (IsGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = groundedStickVelocity;
            }
            else
            {
                _verticalVelocity += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
            }

            return new Vector3(CurrentVelocity.x, _verticalVelocity, CurrentVelocity.z);
        }

        private CollisionFlags ApplyMovement(Vector3 fullVelocity) =>
            _characterController.Move(fullVelocity * Time.deltaTime);

        private void HandleVerticalCollision(CollisionFlags collisionFlags)
        {
            if ((collisionFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0.01f)
            {
                _verticalVelocity = 0f;
            }
        }

        private Vector2 GetLookInput()
        {
            return new Vector2(LookInput.x * _lookSensitivity.x, LookInput.y * _lookSensitivity.y);
        }

        private void LookUpDown(Vector2 input)
        {
            CurrentPitch -= input.y;
            _camera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);
        }

        private void LookLeftRight(Vector2 input) =>
            transform.Rotate(Vector3.up, input.x);

        private void UpdateCameraFOV()
        {
            float targetFOV = _defaultFOV;

            if (IsSprinting)
            {
                float speedRatio = CurrentSpeed / _sprintSpeed;

                targetFOV = Mathf.Lerp(_defaultFOV, SprintFOV, speedRatio);
            }

            _camera.Lens.FieldOfView = Mathf.Lerp(_camera.Lens.FieldOfView, targetFOV, _cameraFOVSmoothing * Time.deltaTime);
        }

        #endregion
    }
}