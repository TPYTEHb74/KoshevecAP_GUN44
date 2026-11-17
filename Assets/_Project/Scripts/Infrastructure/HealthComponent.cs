using System;
using UnityEngine;

namespace Game.Infrastructure
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;
        public bool IsDead { get; private set; }
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        public event Action<float, float> OnHealthChanged;
        public event Action<Vector3, Vector3, float> OnHitForceApplied;
        public event Action OnDied;

        private void Awake()
        {
            ResetHealth();
        }

        public void Initialize(float maxHealth)
        {
            _maxHealth = maxHealth;
            ResetHealth();
        }

        public void ResetHealth()
        {
            IsDead = false;
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Vector3 pushDirection = default, float pushForce = 0f)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (pushForce > 0f && pushDirection != Vector3.zero)
            {
                OnHitForceApplied?.Invoke(hitPoint, pushDirection.normalized, pushForce);
            }

            if (_currentHealth <= 0f)
            {
                IsDead = true;
                OnDied?.Invoke();
            }
        }
    }
}
