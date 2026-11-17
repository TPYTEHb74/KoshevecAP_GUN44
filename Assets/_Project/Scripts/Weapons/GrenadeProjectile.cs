using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using Zenject;

namespace Game.Weapons
{
    [RequireComponent(typeof(Rigidbody))]
    public class GrenadeProjectile : MonoBehaviour, Game.Infrastructure.IPoolable
    {
        [SerializeField] private float _explosionRadius = 5f;
        [SerializeField] private float _explosionForce = 15f;
        [SerializeField] private float _fuseTime = 3f;
        [SerializeField] private LayerMask _damageMask;
        [SerializeField] private LayerMask _obstacleMask;

        private readonly Collider[] _hitColliders = new Collider[32];
        private IPoolService _poolService;
        private Rigidbody _rigidbody;
        private CancellationTokenSource _fuseCts;
        private float _damage;
        private bool _hasExploded;

        [Inject]
        public void Construct(IPoolService poolService)
        {
            _poolService = poolService;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Launch(Vector3 velocity, float damage)
        {
            _damage = damage;
            _rigidbody.velocity = velocity;
        }

        public void OnSpawned()
        {
            _hasExploded = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            _fuseCts?.Cancel();
            _fuseCts?.Dispose();
            _fuseCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            FuseTimerAsync(_fuseCts.Token).Forget();
        }

        public void OnDespawned()
        {
            _fuseCts?.Cancel();
            _fuseCts?.Dispose();
            _fuseCts = null;
        }

        private async UniTaskVoid FuseTimerAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_fuseTime), cancellationToken: token);
                Explode();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Explode();
        }

        private void Explode()
        {
            if (_hasExploded)
            {
                return;
            }

            _hasExploded = true;
            Vector3 center = transform.position;

            int count = Physics.OverlapSphereNonAlloc(center, _explosionRadius, _hitColliders, _damageMask);
            for (int i = 0; i < count; i++)
            {
                Collider target = _hitColliders[i];
                Vector3 targetPoint = target.bounds.center;

                if (Physics.Linecast(center, targetPoint, _obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (target.TryGetComponent(out IDamageable damageable))
                {
                    float distance = Vector3.Distance(center, targetPoint);
                    float falloff = Mathf.Clamp01(1f - (distance / _explosionRadius));
                    Vector3 pushDir = (targetPoint - center).normalized;

                    damageable.TakeDamage(_damage * falloff, targetPoint, -pushDir, pushDir, _explosionForce * falloff);
                }
            }

            if (_poolService != null)
            {
                _poolService.Despawn(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
