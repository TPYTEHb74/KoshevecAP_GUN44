using DG.Tweening;
using UnityEngine;

namespace Game.Level
{
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 _moveOffset = new Vector3(0f, 0f, 8f);
        [SerializeField] private float _moveDuration = 3f;
        [SerializeField] private float _waitDuration = 1f;

        private Vector3 _lastPosition;
        private Vector3 _deltaMovement;

        private void Start()
        {
            _lastPosition = transform.position;
            Vector3 targetPosition = transform.position + _moveOffset;

            DOTween.Sequence()
                .AppendInterval(_waitDuration)
                .Append(transform.DOMove(targetPosition, _moveDuration).SetEase(Ease.InOutSine))
                .AppendInterval(_waitDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(UpdateType.Fixed)
                .SetLink(gameObject);
        }

        private void FixedUpdate()
        {
            _deltaMovement = transform.position - _lastPosition;
            _lastPosition = transform.position;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out CharacterController controller))
            {
                controller.Move(_deltaMovement);
            }
        }
    }
}
