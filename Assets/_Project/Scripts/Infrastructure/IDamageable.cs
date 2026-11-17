using UnityEngine;

namespace Game.Infrastructure
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Vector3 pushDirection = default, float pushForce = 0f);
    }
}
