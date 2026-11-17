using System;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Weapons
{
    public interface IShootStrategy
    {
        void ExecuteShot(WeaponConfig config, Transform muzzlePoint, Vector3 aimOrigin, Vector3 aimDirection, float currentSpread);
    }

    public class HitscanShootStrategy : IShootStrategy
    {
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
        private readonly IPoolService _poolService;

        public HitscanShootStrategy(IPoolService poolService)
        {
            _poolService = poolService;
        }

        public void ExecuteShot(WeaponConfig config, Transform muzzlePoint, Vector3 aimOrigin, Vector3 aimDirection, float currentSpread)
        {
            int pellets = Mathf.Max(1, config.PelletsCount);

            for (int i = 0; i < pellets; i++)
            {
                Vector3 shotDirection = ApplySpread(aimDirection, currentSpread);
                int hitCount = Physics.RaycastNonAlloc(aimOrigin, shotDirection, _hitBuffer, config.Range, config.HitMask, QueryTriggerInteraction.Ignore);
                Vector3 endPoint = aimOrigin + shotDirection * config.Range;

                if (hitCount > 0)
                {
                    RaycastHit closestHit = GetClosestHit(hitCount);
                    endPoint = closestHit.point;

                    float normalizedDistance = Mathf.Clamp01(closestHit.distance / config.Range);
                    float finalDamage = config.Damage * config.DamageFalloff.Evaluate(normalizedDistance);

                    if (closestHit.collider.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.TakeDamage(finalDamage, closestHit.point, closestHit.normal, shotDirection, config.PushForce);
                    }
                }

                if (config.TracerPrefab != null && _poolService != null)
                {
                    BulletTracer tracer = _poolService.Spawn(config.TracerPrefab, muzzlePoint.position, Quaternion.identity);
                    tracer.Launch(muzzlePoint.position, endPoint);
                }
            }
        }

        private RaycastHit GetClosestHit(int count)
        {
            RaycastHit closest = _hitBuffer[0];
            for (int i = 1; i < count; i++)
            {
                if (_hitBuffer[i].distance < closest.distance)
                {
                    closest = _hitBuffer[i];
                }
            }

            return closest;
        }

        private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
        {
            if (spreadAngle <= 0.01f)
            {
                return direction;
            }

            float randomYaw = UnityEngine.Random.Range(-spreadAngle, spreadAngle);
            float randomPitch = UnityEngine.Random.Range(-spreadAngle, spreadAngle);
            return Quaternion.Euler(randomPitch, randomYaw, 0f) * direction;
        }
    }

    public class ProjectileShootStrategy : IShootStrategy
    {
        private readonly IPoolService _poolService;

        public ProjectileShootStrategy(IPoolService poolService)
        {
            _poolService = poolService;
        }

        public void ExecuteShot(WeaponConfig config, Transform muzzlePoint, Vector3 aimOrigin, Vector3 aimDirection, float currentSpread)
        {
            if (config.ProjectilePrefab == null)
            {
                return;
            }

            GrenadeProjectile projectile = _poolService.Spawn(
                config.ProjectilePrefab,
                muzzlePoint.position,
                Quaternion.LookRotation(aimDirection));

            projectile.Launch(aimDirection * config.ProjectileSpeed, config.Damage);
        }
    }

    public class PiercingBeamShootStrategy : IShootStrategy
    {
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[32];
        private readonly LineRenderer _beamRenderer;

        public PiercingBeamShootStrategy(LineRenderer beamRenderer)
        {
            _beamRenderer = beamRenderer;
        }

        public void ExecuteShot(WeaponConfig config, Transform muzzlePoint, Vector3 aimOrigin, Vector3 aimDirection, float currentSpread)
        {
            int hitCount = Physics.RaycastNonAlloc(aimOrigin, aimDirection, _hitBuffer, config.Range, config.HitMask, QueryTriggerInteraction.Ignore);
            Vector3 endPoint = aimOrigin + aimDirection * config.Range;

            if (hitCount > 0)
            {
                Array.Sort(_hitBuffer, 0, hitCount, RaycastDistanceComparer.Instance);

                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = _hitBuffer[i];
                    if (hit.collider.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.TakeDamage(config.Damage, hit.point, hit.normal, aimDirection, config.PushForce);
                    }
                    else
                    {
                        endPoint = hit.point;
                        break;
                    }
                }
            }

            if (_beamRenderer != null)
            {
                _beamRenderer.SetPosition(0, muzzlePoint.position);
                _beamRenderer.SetPosition(1, endPoint);
            }

            Debug.DrawLine(muzzlePoint.position, endPoint, Color.cyan, 0.5f);
        }

        private sealed class RaycastDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly RaycastDistanceComparer Instance = new RaycastDistanceComparer();
            public int Compare(RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance);
        }
    }
}
