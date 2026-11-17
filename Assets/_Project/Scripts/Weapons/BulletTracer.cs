using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using Zenject;

namespace Game.Weapons
{
    public class BulletTracer : MonoBehaviour, Game.Infrastructure.IPoolable
    {
        [SerializeField] private float _speed = 120f;
        [SerializeField] private float _maxLifetime = 0.5f;

        private IPoolService _poolService;
        private CancellationTokenSource _lifeCts;
        private Vector3 _targetPoint;
        private bool _isInitialized;

        [Inject]
        public void Construct(IPoolService poolService)
        {
            _poolService = poolService;
        }

        public void Launch(Vector3 startPoint, Vector3 targetPoint)
        {
            transform.position = startPoint;
            _targetPoint = targetPoint;
            transform.LookAt(targetPoint);
            _isInitialized = true;
        }

        public void OnSpawned()
        {
            _isInitialized = false;
            _lifeCts?.Cancel();
            _lifeCts?.Dispose();
            _lifeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            AutoDespawnAsync(_lifeCts.Token).Forget();
        }

        public void OnDespawned()
        {
            _isInitialized = false;
            _lifeCts?.Cancel();
            _lifeCts?.Dispose();
            _lifeCts = null;
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, _targetPoint, _speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPoint) < 0.1f)
            {
                DespawnSelf();
            }
        }

        private async UniTaskVoid AutoDespawnAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_maxLifetime), cancellationToken: token);
                DespawnSelf();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void DespawnSelf()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
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
