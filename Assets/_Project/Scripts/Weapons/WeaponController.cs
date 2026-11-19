using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapons Arsenal (5 Firearms)")]
        [SerializeField] private List<WeaponConfig> _weapons;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private LineRenderer _railgunLineRenderer;
        [SerializeField] private AudioSource _audioSource;

        [Header("Melee Attack (6th Weapon - Key V)")]
        [SerializeField] private float _meleeDamage = 55f;
        [SerializeField] private float _backstabMultiplier = 2f;
        [SerializeField] private float _meleeRange = 2.2f;
        [SerializeField] private float _meleeRadius = 1.2f;
        [SerializeField] private float _meleeCooldown = 0.6f;
        [SerializeField] private LayerMask _meleeHitMask;

        private readonly Collider[] _meleeHits = new Collider[16];

        private IInputService _inputService;
        private IPoolService _poolService;
        private Transform _cameraTransform;

        private Dictionary<ReloadType, IShootStrategy> _strategies;
        private int[] _currentAmmo;
        private int[] _reserveAmmo;
        private float[] _currentHeat;
        private bool[] _isOverheated;

        private int _activeIndex;
        private float _nextFireTime;
        private float _nextMeleeTime;
        private float _currentSpread;
        private bool _isFiring;
        private bool _isReloading;

        private CancellationTokenSource _reloadCts;

        public event Action<WeaponConfig, int, int> OnAmmoChanged;
        public event Action<float, bool> OnHeatChanged;
        public event Action<bool> OnReloadStateChanged;

        public WeaponConfig CurrentWeapon => _weapons != null && _weapons.Count > 0 ? _weapons[_activeIndex] : null;

        [Inject]
        public void Construct(IInputService inputService, IPoolService poolService)
        {
            _inputService = inputService;
            _poolService = poolService;
        }

        private void Awake()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            InitializeArsenal();
        }

        private void OnEnable()
        {
            _inputService.OnFirePressed += HandleFirePressed;
            _inputService.OnFireReleased += HandleFireReleased;
            _inputService.OnReloadPressed += HandleReloadPressed;
            _inputService.OnMeleePressed += PerformMeleeAttack;
            _inputService.OnWeaponSlotSelected += SelectWeapon;
            _inputService.OnWeaponScrolled += HandleWeaponScroll;
        }

        private void OnDisable()
        {
            _inputService.OnFirePressed -= HandleFirePressed;
            _inputService.OnFireReleased -= HandleFireReleased;
            _inputService.OnReloadPressed -= HandleReloadPressed;
            _inputService.OnMeleePressed -= PerformMeleeAttack;
            _inputService.OnWeaponSlotSelected -= SelectWeapon;
            _inputService.OnWeaponScrolled -= HandleWeaponScroll;
            CancelReload();
        }

        private void Start()
        {
            if (_weapons != null && _weapons.Count > 0)
            {
                SelectWeapon(0);
            }
        }

        private void Update()
        {
            WeaponConfig weapon = CurrentWeapon;
            if (weapon == null)
            {
                return;
            }

            _currentSpread = Mathf.MoveTowards(_currentSpread, weapon.BaseSpreadAngle, weapon.SpreadRecoverySpeed * Time.deltaTime);

            if (weapon.ReloadType == ReloadType.Overheat && _currentHeat[_activeIndex] > 0f)
            {
                _currentHeat[_activeIndex] = Mathf.Max(0f, _currentHeat[_activeIndex] - weapon.CoolingRate * Time.deltaTime);
                if (_currentHeat[_activeIndex] <= 0f)
                {
                    _isOverheated[_activeIndex] = false;
                }

                OnHeatChanged?.Invoke(_currentHeat[_activeIndex], _isOverheated[_activeIndex]);
            }

            if (_isFiring && weapon.IsAutomatic)
            {
                TryShoot();
            }
        }

        private void InitializeArsenal()
        {
            int count = _weapons.Count;
            _currentAmmo = new int[count];
            _reserveAmmo = new int[count];
            _currentHeat = new float[count];
            _isOverheated = new bool[count];

            for (int i = 0; i < count; i++)
            {
                _currentAmmo[i] = _weapons[i].MagazineSize;
                _reserveAmmo[i] = _weapons[i].MaxReserveAmmo;
            }

            IShootStrategy hitscan = new HitscanShootStrategy(_poolService);
            IShootStrategy projectile = new ProjectileShootStrategy(_poolService);
            IShootStrategy beam = new PiercingBeamShootStrategy(_railgunLineRenderer);

            _strategies = new Dictionary<ReloadType, IShootStrategy>
            {
                { ReloadType.Magazine, hitscan },
                { ReloadType.SingleShell, hitscan },
                { ReloadType.Overheat, beam }
            };
        }

        private void HandleFirePressed()
        {
            _isFiring = true;
            TryShoot();
        }

        private void HandleFireReleased()
        {
            _isFiring = false;
        }

        private void TryShoot()
        {
            WeaponConfig weapon = CurrentWeapon;
            if (weapon == null || Time.time < _nextFireTime)
            {
                return;
            }

            if (_isReloading)
            {
                if (weapon.ReloadType == ReloadType.SingleShell && _currentAmmo[_activeIndex] > 0)
                {
                    CancelReload();
                }
                else
                {
                    return;
                }
            }

            if (weapon.ReloadType == ReloadType.Overheat)
            {
                if (_isOverheated[_activeIndex])
                {
                    return;
                }

                _currentHeat[_activeIndex] = Mathf.Min(1f, _currentHeat[_activeIndex] + weapon.HeatPerShot);
                if (_currentHeat[_activeIndex] >= 1f)
                {
                    _isOverheated[_activeIndex] = true;
                }

                OnHeatChanged?.Invoke(_currentHeat[_activeIndex], _isOverheated[_activeIndex]);
            }
            else
            {
                if (_currentAmmo[_activeIndex] <= 0)
                {
                    HandleReloadPressed();
                    return;
                }

                _currentAmmo[_activeIndex]--;
                NotifyAmmoChanged();
            }

            _nextFireTime = Time.time + weapon.FireRate;

            Vector3 aimOrigin = _cameraTransform != null ? _cameraTransform.position : _muzzlePoint.position;
            Vector3 aimDirection = _cameraTransform != null ? _cameraTransform.forward : transform.forward;

            IShootStrategy strategy = weapon.FireType == WeaponFireType.Projectile
                ? new ProjectileShootStrategy(_poolService)
                : _strategies[weapon.ReloadType];

            strategy.ExecuteShot(weapon, _muzzlePoint, aimOrigin, aimDirection, _currentSpread);
            _currentSpread = Mathf.Min(weapon.MaxSpreadAngle, _currentSpread + weapon.SpreadIncreasePerShot);

            if (_audioSource != null && weapon.FireSound != null)
            {
                _audioSource.pitch = UnityEngine.Random.Range(0.92f, 1.08f);
                _audioSource.PlayOneShot(weapon.FireSound);
            }

        }

        private void HandleReloadPressed()
        {
            WeaponConfig weapon = CurrentWeapon;
            if (weapon == null || _isReloading || weapon.ReloadType == ReloadType.Overheat)
            {
                return;
            }

            if (_currentAmmo[_activeIndex] >= weapon.MagazineSize)
            {
                return;
            }

            if (!weapon.InfiniteReserveAmmo && _reserveAmmo[_activeIndex] <= 0)
            {
                return;
            }

            CancelReload();
            _reloadCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ReloadRoutineAsync(weapon, _activeIndex, _reloadCts.Token).Forget();
        }

        private async UniTaskVoid ReloadRoutineAsync(WeaponConfig weapon, int slotIndex, CancellationToken token)
        {
            _isReloading = true;
            OnReloadStateChanged?.Invoke(true);

            try
            {
                if (weapon.ReloadType == ReloadType.SingleShell)
                {
                    while (_currentAmmo[slotIndex] < weapon.MagazineSize &&
                           (weapon.InfiniteReserveAmmo || _reserveAmmo[slotIndex] > 0))
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(weapon.ReloadDuration), cancellationToken: token);

                        _currentAmmo[slotIndex]++;
                        if (!weapon.InfiniteReserveAmmo)
                        {
                            _reserveAmmo[slotIndex]--;
                        }

                        NotifyAmmoChanged();
                    }
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(weapon.ReloadDuration), cancellationToken: token);

                    int needed = weapon.MagazineSize - _currentAmmo[slotIndex];
                    int toLoad = weapon.InfiniteReserveAmmo ? needed : Mathf.Min(needed, _reserveAmmo[slotIndex]);

                    _currentAmmo[slotIndex] += toLoad;
                    if (!weapon.InfiniteReserveAmmo)
                    {
                        _reserveAmmo[slotIndex] -= toLoad;
                    }
                    NotifyAmmoChanged();
                }
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                _isReloading = false;
                OnReloadStateChanged?.Invoke(false);
            }
        }
        private void CancelReload()
        {
            if (_reloadCts != null)
            {
                _reloadCts.Cancel();
                _reloadCts.Dispose();
                _reloadCts = null;
            }
            _isReloading = false;
            OnReloadStateChanged?.Invoke(false);
        }
        public void SelectWeapon(int index)
        {
            if (_weapons == null || index < 0 || index >= _weapons.Count)
            {
                return;
            }
            CancelReload();
            _activeIndex = index;
            _currentSpread = CurrentWeapon.BaseSpreadAngle;
            NotifyAmmoChanged();
        }
        private void HandleWeaponScroll(float scrollValue)
        {
            if (_weapons == null || _weapons.Count == 0 || Mathf.Approximately(scrollValue, 0f))
            {
                return;
            }
            int nextIndex = (_activeIndex + (scrollValue > 0f ? 1 : -1) + _weapons.Count) % _weapons.Count;
            SelectWeapon(nextIndex);
        }
        private void PerformMeleeAttack()
        {
            if (Time.time < _nextMeleeTime)
            {
                return;
            }
            _nextMeleeTime = Time.time + _meleeCooldown;
            Vector3 attackOrigin = transform.position + Vector3.up * 1.1f + transform.forward * (_meleeRange * 0.5f);
            int hitCount = Physics.OverlapSphereNonAlloc(attackOrigin, _meleeRadius, _meleeHits, _meleeHitMask);
            for (int i = 0; i < hitCount; i++)
            {
                Collider target = _meleeHits[i];
                if (target.transform == transform)
                {
                    continue;
                }
                Vector3 toTarget = (target.transform.position - transform.position).normalized;
                if (Vector3.Dot(transform.forward, toTarget) < 0.5f)
                {
                    continue;
                }
                if (target.TryGetComponent(out IDamageable damageable))
                {
                    bool isBackstab = Vector3.Dot(transform.forward, target.transform.forward) > 0.6f;
                    float damage = isBackstab ? _meleeDamage * _backstabMultiplier : _meleeDamage;
                    damageable.TakeDamage(damage, target.bounds.center, -toTarget, transform.forward, 8f);
                }
            }
        }
        public void AddAmmo(int amount)
        {
            WeaponConfig weapon = CurrentWeapon;
            if (weapon == null || weapon.InfiniteReserveAmmo || weapon.ReloadType == ReloadType.Overheat)
            {
                return;
            }
            _reserveAmmo[_activeIndex] = Mathf.Min(weapon.MaxReserveAmmo, _reserveAmmo[_activeIndex] + amount);
            NotifyAmmoChanged();
        }
        private void NotifyAmmoChanged()
        {
            WeaponConfig weapon = CurrentWeapon;
            if (weapon != null)
            {
                OnAmmoChanged?.Invoke(weapon, _currentAmmo[_activeIndex], _reserveAmmo[_activeIndex]);
            }
        }
    }
}