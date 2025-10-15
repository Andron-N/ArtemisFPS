using UnityEngine;
using Unity.Cinemachine;


namespace ArtemisFPS.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class Controller : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _gravityMultiplier = 20f;

        [SerializeField] private float _walkSpeed = 3.5f;
        [SerializeField] private float _sprintSpeed = 8f;

        private Vector3 CurrentVelocity { get; set; }
        private float CurrentSpeed { get; set; }
        private float MaxSpeed => IsSprintInput ? _sprintSpeed : _walkSpeed;

        private bool IsSprinting => IsSprintInput && CurrentSpeed > 0.1f;


        [Header("Looking Parameters")]
        [SerializeField] private Vector2 _lookSensitivity = new Vector2(0.1f, 0.1f);
        [SerializeField] private float _pitchLimit = 85f;

        [SerializeField] private float _currentPitch;

        private float CurrentPitch
        {
            get => _currentPitch;
            set => _currentPitch = Mathf.Clamp(value, -_pitchLimit, _pitchLimit);
        }


        [Header("Camera Parameters")]
        [SerializeField] private float _cameraNormalFOV = 90f;
        [SerializeField, Range(1f, 10f)] private float _cameraFOVSmoothing = 3f;

        private float CameraSprintFOV => _cameraNormalFOV * 1.1f;
        private float TargetCameraFOV => IsSprintInput ? CameraSprintFOV : _cameraNormalFOV;


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

        private void MoveUpdate()
        {
            Vector3 motion = GetMovementVector();
            UpdateVelocity(motion);

            Vector3 fullVelocity = GetFullVelocity();
            Move(fullVelocity);

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
            Vector3 target = motion * MaxSpeed;

            if (motion.sqrMagnitude < 0.01f)
                target = Vector3.zero;

            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, target, _acceleration * Time.deltaTime);
        }

        private Vector3 GetFullVelocity()
        {
            float verticalVelocity = Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
            return new Vector3(CurrentVelocity.x, verticalVelocity, CurrentVelocity.z);
        }

        private void Move(Vector3 motion)
        {
            _characterController.Move(motion * Time.deltaTime);
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

        private void LookLeftRight(Vector2 input)
        {
            transform.Rotate(Vector3.up, input.x);
        }

        private void UpdateCameraFOV()
        {
            float targetFOV = _cameraNormalFOV;

            if (IsSprinting)
            {
                float speedRatio = CurrentSpeed / _sprintSpeed;

                targetFOV = Mathf.Lerp(_cameraNormalFOV, CameraSprintFOV, speedRatio);
            }

            _camera.Lens.FieldOfView = Mathf.Lerp(_camera.Lens.FieldOfView, targetFOV, _cameraFOVSmoothing * Time.deltaTime);
        }

        #endregion
    }
}