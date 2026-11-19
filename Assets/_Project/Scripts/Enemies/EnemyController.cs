using DG.Tweening;
using Game.Infrastructure;
using Game.Player;
using Game.Weapons;
using System;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Enemies
{
    [RequireComponent(typeof(NavMeshAgent), typeof(HealthComponent))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig _config;
        [SerializeField] private Transform _eyePoint;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private BulletTracer _tracerPrefab;
        [SerializeField] private Transform[] _patrolPoints;

        private IEnemyState _currentState;
        private PlayerController _player;
        private IPoolService _poolService;
        private CharacterController _playerCharacterController;
        private HealthComponent _health;
        private Renderer _renderer;
        private Collider _collider;

        private float _nextVisionCheckTime;
        private bool _canSeeTarget;
        private bool _hasAggro;

        public EnemyConfig Config => _config;
        public NavMeshAgent Agent { get; private set; }
        public Transform[] PatrolPoints => _patrolPoints;
        public Transform TargetTransform => _player != null ? _player.transform : null;
        public bool HasTarget => _player != null && (_canSeeTarget || _hasAggro);
        public bool CanSeeTarget => _canSeeTarget;
        public float DistanceToTarget => _player != null ? Vector3.Distance(transform.position, _player.transform.position) : float.MaxValue;

        public EnemyPatrolState PatrolState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyDeadState DeadState { get; private set; }

        public event Action<EnemyController> OnEnemyDied;

        [Inject]
        public void Construct(PlayerController player, IPoolService poolService)
        {
            _player = player;
            _poolService = poolService;
            if (_player != null)
            {
                _playerCharacterController = _player.GetComponent<CharacterController>();
            }
        }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<HealthComponent>();
            _renderer = GetComponentInChildren<Renderer>();
            _collider = GetComponent<Collider>();

            PatrolState = new EnemyPatrolState(this);
            ChaseState = new EnemyChaseState(this);
            AttackState = new EnemyAttackState(this);
            DeadState = new EnemyDeadState(this);
        }

        private void OnEnable()
        {
            _health.Initialize(_config.MaxHealth);
            _health.OnHealthChanged += HandleHealthChanged;
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnHealthChanged -= HandleHealthChanged;
            _health.OnDied -= HandleDied;
        }

        private void Start()
        {
            ChangeState(PatrolState);
        }

        private void Update()
        {
            if (_health.IsDead)
            {
                return;
            }

            if (Time.time >= _nextVisionCheckTime)
            {
                _nextVisionCheckTime = Time.time + _config.VisionCheckInterval;
                UpdatePerception();
            }

            _currentState?.Tick();
        }

        public void SetPatrolPoints(Transform[] points)
        {
            if (points != null && points.Length > 0)
            {
                _patrolPoints = points;
            }
        }

        public void ChangeState(IEnemyState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        private void UpdatePerception()
        {
            if (_player == null)
            {
                _canSeeTarget = false;
                return;
            }

            Vector3 eyePos = _eyePoint != null ? _eyePoint.position : transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = _player.transform.position + Vector3.up * 1.1f;
            Vector3 dirToTarget = targetPos - eyePos;
            float distance = dirToTarget.magnitude;

            if (distance > _config.ViewRadius)
            {
                _canSeeTarget = false;
                return;
            }

            if (distance <= _config.HearingRadius)
            {
                _canSeeTarget = !Physics.Linecast(eyePos, targetPos, _config.ObstacleMask, QueryTriggerInteraction.Ignore);
                if (_canSeeTarget)
                {
                    _hasAggro = true;
                }

                return;
            }

            float angle = Vector3.Angle(transform.forward, dirToTarget.normalized);
            if (angle > _config.ViewAngle * 0.5f)
            {
                _canSeeTarget = false;
                return;
            }

            if (Physics.Linecast(eyePos, targetPos, _config.ObstacleMask, QueryTriggerInteraction.Ignore))
            {
                _canSeeTarget = false;
                return;
            }

            _canSeeTarget = true;
            _hasAggro = true;
        }

        public void RotateTowardsTarget()
        {
            if (_player == null)
            {
                return;
            }

            Vector3 direction = (_player.transform.position - transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _config.RotationSpeed);
            }
        }

        public void PerformAttack()
        {
            if (_player == null)
            {
                return;
            }

            if (_config.EnemyType == EnemyType.MeleeRusher)
            {
                if (DistanceToTarget <= _config.AttackRange * 1.2f && _player.TryGetComponent(out IDamageable damageable))
                {
                    Vector3 pushDir = (_player.transform.position - transform.position).normalized;
                    damageable.TakeDamage(_config.AttackDamage, _player.transform.position, -pushDir, pushDir, 4f);
                }
            }
            else
            {
                Vector3 origin = _muzzlePoint != null ? _muzzlePoint.position : transform.position + Vector3.up * 1.3f;
                Vector3 targetCenter = _player.transform.position + Vector3.up * 1.1f;

                if (_playerCharacterController != null)
                {
                    targetCenter += _playerCharacterController.velocity * _config.LeadPredictionFactor;
                }

                Vector3 shotDir = (targetCenter - origin).normalized;
                Vector3 endPoint = origin + shotDir * _config.AttackRange;

                if (Physics.Raycast(origin, shotDir, out RaycastHit hit, _config.AttackRange, _config.TargetMask | _config.ObstacleMask, QueryTriggerInteraction.Ignore))
                {
                    endPoint = hit.point;
                    if (hit.collider.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.TakeDamage(_config.AttackDamage, hit.point, hit.normal, shotDir, 2f);
                    }
                }

                if (_tracerPrefab != null && _poolService != null)
                {
                    BulletTracer tracer = _poolService.Spawn(_tracerPrefab, origin, Quaternion.identity);
                    tracer.Launch(origin, endPoint);
                }
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            _hasAggro = true;
            if (_renderer != null)
            {
                _renderer.material.color = Color.Lerp(Color.black, Color.red, current / max);
            }
        }

        private void HandleDied()
        {
            ChangeState(DeadState);
            OnEnemyDied?.Invoke(this);
        }

        public void HandleDeathVisuals()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            if (_renderer != null)
            {
                _renderer.material.color = Color.gray;
            }

            transform.localScale = new Vector3(1f, 0.3f, 1f);
            transform.DOScale(Vector3.zero, 0.5f).SetDelay(2f).SetLink(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null)
            {
                return;
            }

            Gizmos.color = _currentState != null ? _currentState.GizmoColor : Color.cyan;
            Vector3 pos = transform.position + Vector3.up * 0.2f;

            Gizmos.DrawWireSphere(pos, _config.ViewRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pos, _config.AttackRange);

            Gizmos.color = Color.yellow;
            Vector3 leftBoundary = Quaternion.Euler(0f, -_config.ViewAngle * 0.5f, 0f) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0f, _config.ViewAngle * 0.5f, 0f) * transform.forward;
            Gizmos.DrawRay(pos, leftBoundary * _config.ViewRadius);
            Gizmos.DrawRay(pos, rightBoundary * _config.ViewRadius);
            if (_patrolPoints != null && _patrolPoints.Length > 1)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < _patrolPoints.Length; i++)
                {
                    if (_patrolPoints[i] != null && _patrolPoints[(i + 1) % _patrolPoints.Length] != null)
                    {
                        Gizmos.DrawLine(_patrolPoints[i].position, _patrolPoints[(i + 1) % _patrolPoints.Length].position);
                    }
                }
            }
        }
    }
}