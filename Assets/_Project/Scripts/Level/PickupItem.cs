using DG.Tweening;
using Game.Infrastructure;
using Game.Player;
using Game.Weapons;
using UnityEngine;

namespace Game.Level
{
    public enum PickupType
    {
        Health,
        Ammo,
        KeyCard
    }

    public class PickupItem : MonoBehaviour
    {
        [SerializeField] private PickupType _type = PickupType.Health;
        [SerializeField] private float _amount = 30f;
        [SerializeField] private string _keyId = "Zone1Key";

        private bool _isCollected;

        private void Start()
        {
            transform.DORotate(new Vector3(0f, 360f, 0f), 3f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);

            transform.DOMoveY(transform.position.y + 0.25f, 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected)
            {
                return;
            }

            bool applied = false;

            switch (_type)
            {
                case PickupType.Health:
                    if (other.TryGetComponent(out HealthComponent health) && health.CurrentHealth < health.MaxHealth)
                    {
                        health.Heal(_amount);
                        applied = true;
                    }
                    break;

                case PickupType.Ammo:
                    if (other.TryGetComponent(out WeaponController weapons))
                    {
                        weapons.AddAmmo(Mathf.RoundToInt(_amount));
                        applied = true;
                    }
                    break;

                case PickupType.KeyCard:
                    if (other.TryGetComponent(out PlayerInventory inventory))
                    {
                        inventory.AddKey(_keyId);
                        applied = true;
                    }
                    break;
            }

            if (applied)
            {
                _isCollected = true;
                transform.DOKill();
                transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .SetLink(gameObject)
                    .OnComplete(() => Destroy(gameObject));
            }
        }
    }
}
