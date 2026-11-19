using DG.Tweening;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Level
{
    public class HazardTrap : MonoBehaviour
    {
        [SerializeField] private Transform _movingPart;
        [SerializeField] private Vector3 _activeOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private float _strikeDuration = 0.25f;
        [SerializeField] private float _retractDuration = 1.0f;
        [SerializeField] private float _idleDelay = 1.5f;
        [SerializeField] private float _damage = 35f;

        private void Start()
        {
            if (_movingPart == null)
            {
                return;
            }

            Vector3 startPos = _movingPart.localPosition;
            DOTween.Sequence()
                .AppendInterval(_idleDelay)
                .Append(_movingPart.DOLocalMove(startPos + _activeOffset, _strikeDuration).SetEase(Ease.InExpo))
                .AppendInterval(0.3f)
                .Append(_movingPart.DOLocalMove(startPos, _retractDuration).SetEase(Ease.OutQuad))
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                damageable.TakeDamage(_damage, other.bounds.center, Vector3.up, pushDir, 10f);
            }
        }
    }
}
