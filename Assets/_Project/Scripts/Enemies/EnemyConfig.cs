using UnityEngine;

namespace Game.Enemies
{
    public enum EnemyType
    {
        MeleeRusher,
        RangedShooter
    }

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private EnemyType _enemyType = EnemyType.MeleeRusher;
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _patrolSpeed = 2.5f;
        [SerializeField] private float _chaseSpeed = 5.0f;
        [SerializeField] private float _rotationSpeed = 8f;

        [Header("Perception (Field of View)")]
        [SerializeField] private float _viewRadius = 18f;
        [SerializeField][Range(10f, 360f)] private float _viewAngle = 110f;
        [SerializeField] private float _hearingRadius = 6f;
        [SerializeField] private float _visionCheckInterval = 0.15f;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _obstacleMask;

        [Header("Combat")]
        [SerializeField] private float _attackRange = 2.2f;
        [SerializeField] private float _preferredCombatDistance = 12f;
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _attackCooldown = 1.2f;
        [SerializeField] private float _leadPredictionFactor = 0.35f;

        public EnemyType EnemyType => _enemyType;
        public float MaxHealth => _maxHealth;
        public float PatrolSpeed => _patrolSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float ViewRadius => _viewRadius;
        public float ViewAngle => _viewAngle;
        public float HearingRadius => _hearingRadius;
        public float VisionCheckInterval => _visionCheckInterval;
        public LayerMask TargetMask => _targetMask;
        public LayerMask ObstacleMask => _obstacleMask;
        public float AttackRange => _attackRange;
        public float PreferredCombatDistance => _preferredCombatDistance;
        public float AttackDamage => _attackDamage;
        public float AttackCooldown => _attackCooldown;
        public float LeadPredictionFactor => _leadPredictionFactor;
    }
}
