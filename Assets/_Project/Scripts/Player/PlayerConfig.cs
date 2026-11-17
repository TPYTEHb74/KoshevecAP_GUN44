using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;

        [Header("Movement Speeds")]
        [SerializeField] private float _walkSpeed = 4.5f;
        [SerializeField] private float _sprintSpeed = 7.5f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _aimSpeed = 3.0f;
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        [Header("Jump & Gravity")]
        [SerializeField] private float _jumpHeight = 1.4f;
        [SerializeField] private float _gravity = -19.62f;
        [SerializeField] private float _groundCheckRadius = 0.25f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        [SerializeField] private float _maxSlopeAngle = 45f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Crouch Settings")]
        [SerializeField] private float _standingHeight = 1.8f;
        [SerializeField] private float _crouchingHeight = 1.0f;
        [SerializeField] private float _crouchTransitionSpeed = 10f;

        [Header("Camera Look")]
        [SerializeField] private float _mouseSensitivity = 0.15f;
        [SerializeField] private float _aimSensitivityMultiplier = 0.5f;
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 70f;

        public float MaxHealth => _maxHealth;
        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float AimSpeed => _aimSpeed;
        public float RotationSmoothTime => _rotationSmoothTime;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float GroundCheckRadius => _groundCheckRadius;
        public float GroundCheckDistance => _groundCheckDistance;
        public float MaxSlopeAngle => _maxSlopeAngle;
        public LayerMask GroundLayer => _groundLayer;
        public float StandingHeight => _standingHeight;
        public float CrouchingHeight => _crouchingHeight;
        public float CrouchTransitionSpeed => _crouchTransitionSpeed;
        public float MouseSensitivity => _mouseSensitivity;
        public float AimSensitivityMultiplier => _aimSensitivityMultiplier;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
    }
}
