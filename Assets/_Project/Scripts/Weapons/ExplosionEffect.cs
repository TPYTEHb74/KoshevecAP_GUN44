using DG.Tweening;
using UnityEngine;

namespace Game.Weapons
{
    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private float _maxScale = 8f;
        [SerializeField] private float _duration = 0.3f;

        private void Start()
        {
            transform.localScale = Vector3.one * 0.2f;
            transform.DOScale(Vector3.one * _maxScale, _duration)
                .SetEase(Ease.OutExpo)
                .SetLink(gameObject)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
