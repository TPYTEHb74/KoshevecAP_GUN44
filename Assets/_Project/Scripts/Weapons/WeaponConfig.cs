using UnityEngine;

namespace Game.Weapons
{
    public enum WeaponFireType
    {
        Hitscan,
        Shotgun,
        Projectile,
        PiercingBeam
    }

    public enum ReloadType
    {
        Magazine,
        SingleShell,
        Overheat
    }

    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private string _weaponName = "Pistol";
        [SerializeField] private WeaponFireType _fireType = WeaponFireType.Hitscan;
        [SerializeField] private ReloadType _reloadType = ReloadType.Magazine;
        [SerializeField] private bool _isAutomatic = false;
        [SerializeField] private bool _infiniteReserveAmmo = false;

        [Header("Damage & Ballistics")]
        [SerializeField] private float _damage = 25f;
        [SerializeField] private float _fireRate = 0.25f;
        [SerializeField] private float _range = 100f;
        [SerializeField] private float _pushForce = 5f;
        [SerializeField] private int _pelletsCount = 1;
        [SerializeField] private float _baseSpreadAngle = 0.5f;
        [SerializeField] private float _maxSpreadAngle = 4f;
        [SerializeField] private float _spreadIncreasePerShot = 0.5f;
        [SerializeField] private float _spreadRecoverySpeed = 3f;
        [SerializeField] private AnimationCurve _damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private BulletTracer _tracerPrefab;

        [Header("Ammo & Reload / Overheat")]
        [SerializeField] private int _magazineSize = 12;
        [SerializeField] private int _maxReserveAmmo = 60;
        [SerializeField] private float _reloadDuration = 1.5f;
        [SerializeField] private float _heatPerShot = 0.2f;
        [SerializeField] private float _coolingRate = 0.35f;
        [SerializeField] private float _overheatCooldownDuration = 2.5f;

        [Header("Projectile Settings (For Grenade Launcher)")]
        [SerializeField] private GrenadeProjectile _projectilePrefab;
        [SerializeField] private float _projectileSpeed = 25f;
      
        [Header("Audio")]
        [SerializeField] private AudioClip _fireSound;
      
        public string WeaponName => _weaponName;
        public WeaponFireType FireType => _fireType;
        public ReloadType ReloadType => _reloadType;
        public bool IsAutomatic => _isAutomatic;
        public bool InfiniteReserveAmmo => _infiniteReserveAmmo;
        public float Damage => _damage;
        public float FireRate => _fireRate;
        public float Range => _range;
        public float PushForce => _pushForce;
        public int PelletsCount => _pelletsCount;
        public float BaseSpreadAngle => _baseSpreadAngle;
        public float MaxSpreadAngle => _maxSpreadAngle;
        public float SpreadIncreasePerShot => _spreadIncreasePerShot;
        public float SpreadRecoverySpeed => _spreadRecoverySpeed;
        public AnimationCurve DamageFalloff => _damageFalloff;
        public LayerMask HitMask => _hitMask;
        public int MagazineSize => _magazineSize;
        public int MaxReserveAmmo => _maxReserveAmmo;
        public float ReloadDuration => _reloadDuration;
        public float HeatPerShot => _heatPerShot;
        public float CoolingRate => _coolingRate;
        public float OverheatCooldownDuration => _overheatCooldownDuration;
        public GrenadeProjectile ProjectilePrefab => _projectilePrefab;
        public float ProjectileSpeed => _projectileSpeed;
        public BulletTracer TracerPrefab => _tracerPrefab;
        public AudioClip FireSound => _fireSound;
    }
}
