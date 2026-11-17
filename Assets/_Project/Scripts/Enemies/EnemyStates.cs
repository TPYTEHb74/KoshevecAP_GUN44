using UnityEngine;

namespace Game.Enemies
{
    public interface IEnemyState
    {
        Color GizmoColor { get; }
        void Enter();
        void Tick();
        void Exit();
    }

    public class EnemyPatrolState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private int _currentWaypointIndex;

        public Color GizmoColor => Color.green;

        public EnemyPatrolState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = false;
            _enemy.Agent.speed = _enemy.Config.PatrolSpeed;
            MoveToNextWaypoint();
        }

        public void Tick()
        {
            if (_enemy.HasTarget)
            {
                _enemy.ChangeState(_enemy.ChaseState);
                return;
            }

            if (_enemy.PatrolPoints == null || _enemy.PatrolPoints.Length == 0)
            {
                return;
            }

            if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= 0.5f)
            {
                MoveToNextWaypoint();
            }
        }

        public void Exit()
        {
        }

        private void MoveToNextWaypoint()
        {
            if (_enemy.PatrolPoints == null || _enemy.PatrolPoints.Length == 0)
            {
                return;
            }

            _enemy.Agent.SetDestination(_enemy.PatrolPoints[_currentWaypointIndex].position);
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _enemy.PatrolPoints.Length;
        }
    }

    public class EnemyChaseState : IEnemyState
    {
        private readonly EnemyController _enemy;

        public Color GizmoColor => Color.yellow;

        public EnemyChaseState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = false;
            _enemy.Agent.speed = _enemy.Config.ChaseSpeed;
        }

        public void Tick()
        {
            if (!_enemy.HasTarget)
            {
                _enemy.ChangeState(_enemy.PatrolState);
                return;
            }

            float distance = _enemy.DistanceToTarget;
            if (distance <= _enemy.Config.AttackRange && _enemy.CanSeeTarget)
            {
                _enemy.ChangeState(_enemy.AttackState);
                return;
            }

            _enemy.Agent.SetDestination(_enemy.TargetTransform.position);
        }

        public void Exit()
        {
        }
    }

    public class EnemyAttackState : IEnemyState
    {
        private readonly EnemyController _enemy;
        private float _nextAttackTime;
        private float _strafeTimer;
        private Vector3 _strafeOffset;

        public Color GizmoColor => Color.red;

        public EnemyAttackState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _strafeTimer = 0f;
        }

        public void Tick()
        {
            if (!_enemy.HasTarget)
            {
                _enemy.ChangeState(_enemy.PatrolState);
                return;
            }

            float distance = _enemy.DistanceToTarget;
            if (distance > _enemy.Config.AttackRange * 1.25f || !_enemy.CanSeeTarget)
            {
                _enemy.ChangeState(_enemy.ChaseState);
                return;
            }

            _enemy.RotateTowardsTarget();

            if (_enemy.Config.EnemyType == EnemyType.MeleeRusher)
            {
                _enemy.Agent.isStopped = false;
                _enemy.Agent.SetDestination(_enemy.TargetTransform.position);
            }
            else
            {
                HandleRangedPositioning(distance);
            }

            if (Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + _enemy.Config.AttackCooldown;
                _enemy.PerformAttack();
            }
        }

        public void Exit()
        {
            _enemy.Agent.isStopped = false;
        }

        private void HandleRangedPositioning(float currentDistance)
        {
            _strafeTimer -= Time.deltaTime;
            if (_strafeTimer <= 0f)
            {
                _strafeTimer = Random.Range(1.2f, 2.2f);
                float side = Random.value > 0.5f ? 1f : -1f;
                _strafeOffset = _enemy.transform.right * (side * 3f);
            }

            float preferred = _enemy.Config.PreferredCombatDistance;
            Vector3 desiredPos = _enemy.transform.position + _strafeOffset;

            if (currentDistance < preferred * 0.7f)
            {
                desiredPos -= _enemy.transform.forward * 4f;
            }

            _enemy.Agent.isStopped = false;
            _enemy.Agent.SetDestination(desiredPos);
        }
    }

    public class EnemyDeadState : IEnemyState
    {
        private readonly EnemyController _enemy;

        public Color GizmoColor => Color.gray;

        public EnemyDeadState(EnemyController enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _enemy.Agent.isStopped = true;
            _enemy.Agent.enabled = false;
            _enemy.HandleDeathVisuals();
        }

        public void Tick()
        {
        }

        public void Exit()
        {
        }
    }
}
