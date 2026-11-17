using Cinemachine;
using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private Transform _cameraFollowTarget;
        [SerializeField] private CinemachineVirtualCamera _aimCamera;
        [SerializeField] private Animator _animator;

        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

        private const int AimCameraActivePriority = 20;
        private const int AimCameraInactivePriority = 5;

        private IInputService _inputService;
        private CharacterController _controller;
        private Transform _mainCameraTransform;

        private Vector3 _verticalVelocity;
        private Vector3 _groundNormal = Vector3.up;
        private float _rotationVelocity;
        private float _cameraYaw;
        private float _cameraPitch;
        private bool _isGrounded;
        private bool _isCrouching;

        public bool IsAiming => _inputService.IsAiming;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            _inputService.EnablePlayerInput();
            _inputService.OnJumpPressed += HandleJump;
            _inputService.OnCrouchPressed += HandleCrouchToggle;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            _inputService.OnJumpPressed -= HandleJump;
            _inputService.OnCrouchPressed -= HandleCrouchToggle;
        }

        private void Update()
        {
            CheckGroundAndSlope();
            HandleCrouchHeight();
            HandleMovement();
            HandleAimingCamera();
            UpdateAnimations();
        }

        private void LateUpdate()
        {
            HandleCameraRotation();
        }

        private void CheckGroundAndSlope()
        {
            Vector3 sphereOrigin = transform.position + Vector3.up * (_config.GroundCheckRadius + 0.05f);

            if (Physics.SphereCast(sphereOrigin, _config.GroundCheckRadius, Vector3.down, out RaycastHit hit,
                _config.GroundCheckDistance, _config.GroundLayer, QueryTriggerInteraction.Ignore))
            {
                float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                _isGrounded = slopeAngle <= _config.MaxSlopeAngle;
                _groundNormal = hit.normal;
            }
            else
            {
                _isGrounded = false;
                _groundNormal = Vector3.up;
            }

            if (_isGrounded && _verticalVelocity.y < 0f)
            {
                _verticalVelocity.y = -2f;
            }
        }

        private void HandleMovement()
        {
            Vector2 moveInput = _inputService.MoveAxis;
            Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            float targetSpeed = GetCurrentSpeed(inputDir.magnitude);
            Vector3 moveDirection = Vector3.zero;

            if (inputDir.magnitude >= 0.1f && _mainCameraTransform != null)
            {
                Vector3 camForward = Vector3.ProjectOnPlane(_mainCameraTransform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(_mainCameraTransform.right, Vector3.up).normalized;

                Vector3 worldDirection = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                moveDirection = Vector3.ProjectOnPlane(worldDirection, _groundNormal).normalized;

                if (!_inputService.IsAiming)
                {
                    float targetAngle = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
                    float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocity, _config.RotationSmoothTime);
                    transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                }
            }

            if (_inputService.IsAiming && _mainCameraTransform != null)
            {
                float targetAngle = _mainCameraTransform.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocity, _config.RotationSmoothTime * 0.5f);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }

            _verticalVelocity.y += _config.Gravity * Time.deltaTime;

            Vector3 finalVelocity = moveDirection * targetSpeed + _verticalVelocity;
            _controller.Move(finalVelocity * Time.deltaTime);
        }

        private void HandleCameraRotation()
        {
            Vector2 lookInput = _inputService.LookAxis;
            float sensitivity = _config.MouseSensitivity * (_inputService.IsAiming ? _config.AimSensitivityMultiplier : 1f);

            _cameraYaw += lookInput.x * sensitivity;
            _cameraPitch -= lookInput.y * sensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _config.MinPitch, _config.MaxPitch);

            if (_cameraFollowTarget != null)
            {
                _cameraFollowTarget.rotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            }
        }

        private void HandleAimingCamera()
        {
            if (_aimCamera == null)
            {
                return;
            }

            _aimCamera.Priority = _inputService.IsAiming ? AimCameraActivePriority : AimCameraInactivePriority;
        }

        private void HandleJump()
        {
            if (!_isGrounded || _isCrouching)
            {
                return;
            }

            _verticalVelocity.y = Mathf.Sqrt(_config.JumpHeight * -2f * _config.Gravity);
        }

        private void HandleCrouchToggle()
        {
            if (_isCrouching)
            {
                float checkDistance = _config.StandingHeight - _config.CrouchingHeight;
                Vector3 origin = transform.position + Vector3.up * _config.CrouchingHeight;
                if (Physics.SphereCast(origin, _controller.radius * 0.9f, Vector3.up, out _, checkDistance, _config.GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    return;
                }
            }

            _isCrouching = !_isCrouching;
        }

        private void HandleCrouchHeight()
        {
            float targetHeight = _isCrouching ? _config.CrouchingHeight : _config.StandingHeight;
            _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * _config.CrouchTransitionSpeed);

            Vector3 center = _controller.center;
            center.y = _controller.height * 0.5f;
            _controller.center = center;
        }

        private float GetCurrentSpeed(float inputMagnitude)
        {
            if (inputMagnitude < 0.1f)
            {
                return 0f;
            }

            if (_isCrouching)
            {
                return _config.CrouchSpeed;
            }

            if (_inputService.IsAiming)
            {
                return _config.AimSpeed;
            }

            if (_inputService.IsSprinting)
            {
                return _config.SprintSpeed;
            }

            return _config.WalkSpeed;
        }

        private void UpdateAnimations()
        {
            if (_animator == null)
            {
                return;
            }

            Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            float forwardSpeed = Vector3.Dot(horizontalVelocity, transform.forward) / _config.SprintSpeed;
            float rightSpeed = Vector3.Dot(horizontalVelocity, transform.right) / _config.SprintSpeed;

            _animator.SetFloat(MoveZHash, forwardSpeed, 0.1f, Time.deltaTime);
            _animator.SetFloat(MoveXHash, rightSpeed, 0.1f, Time.deltaTime);
            _animator.SetBool(IsGroundedHash, _isGrounded);
            _animator.SetBool(IsCrouchingHash, _isCrouching);
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null)
            {
                return;
            }

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 sphereOrigin = transform.position + Vector3.up * (_config.GroundCheckRadius + 0.05f);
            Gizmos.DrawWireSphere(sphereOrigin + Vector3.down * _config.GroundCheckDistance, _config.GroundCheckRadius);
        }
    }
}
