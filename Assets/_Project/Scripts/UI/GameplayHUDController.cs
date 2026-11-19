using DG.Tweening;
using Game.Infrastructure;
using Game.Level;
using Game.Player;
using Game.Services;
using Game.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class GameplayHUDController : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private RectTransform _hudRoot;
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _weaponNameText;
        [SerializeField] private TextMeshProUGUI _ammoText;
        [SerializeField] private TextMeshProUGUI _reloadStatusText;
        [SerializeField] private TextMeshProUGUI _interactionPromptText;
        [SerializeField] private TextMeshProUGUI _enemiesRemainingText;

        [Header("Level References")]
        [SerializeField] private LevelZoneManager _zoneManager;
        [SerializeField] private ExtractionZone _extractionZone;

        [Header("Panels (Pause / Victory / Defeat)")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;

        private IInputService _inputService;
        private PlayerController _player;
        private HealthComponent _playerHealth;
        private WeaponController _playerWeapons;
        private PlayerInteractor _playerInteractor;

        private bool _isPaused;
        private bool _isGameEnded;

        [Inject]
        public void Construct(IInputService inputService, PlayerController player)
        {
            _inputService = inputService;
            _player = player;

            if (_player != null)
            {
                _playerHealth = _player.GetComponent<HealthComponent>();
                _playerWeapons = _player.GetComponent<WeaponController>();
                _playerInteractor = _player.GetComponent<PlayerInteractor>();
            }
        }

        private void OnEnable()
        {
            _inputService.OnPausePressed += TogglePause;

            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged += UpdateHealthUI;
                _playerHealth.OnDied += HandlePlayerDied;
            }

            if (_playerWeapons != null)
            {
                _playerWeapons.OnAmmoChanged += UpdateAmmoUI;
                _playerWeapons.OnHeatChanged += UpdateHeatUI;
                _playerWeapons.OnReloadStateChanged += UpdateReloadUI;
            }

            if (_playerInteractor != null)
            {
                _playerInteractor.OnPromptChanged += UpdateInteractionPrompt;
            }

            if (_zoneManager != null)
            {
                _zoneManager.OnRemainingEnemiesChanged += UpdateEnemiesRemaining;
            }

            if (_extractionZone != null)
            {
                _extractionZone.OnLevelCompleted += HandleLevelVictory;
            }
        }

        private void OnDisable()
        {
            _inputService.OnPausePressed -= TogglePause;

            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= UpdateHealthUI;
                _playerHealth.OnDied -= HandlePlayerDied;
            }

            if (_playerWeapons != null)
            {
                _playerWeapons.OnAmmoChanged -= UpdateAmmoUI;
                _playerWeapons.OnHeatChanged -= UpdateHeatUI;
                _playerWeapons.OnReloadStateChanged -= UpdateReloadUI;
            }

            if (_playerInteractor != null)
            {
                _playerInteractor.OnPromptChanged -= UpdateInteractionPrompt;
            }

            if (_zoneManager != null)
            {
                _zoneManager.OnRemainingEnemiesChanged -= UpdateEnemiesRemaining;
            }

            if (_extractionZone != null)
            {
                _extractionZone.OnLevelCompleted -= HandleLevelVictory;
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;
            SetPanelState(_pausePanel, false);
            SetPanelState(_victoryPanel, false);
            SetPanelState(_defeatPanel, false);
            UpdateInteractionPrompt(string.Empty);
            UpdateReloadUI(false);

            if (_playerHealth != null)
            {
                UpdateHealthUI(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (_healthBar != null)
            {
                _healthBar.maxValue = max;
                _healthBar.value = current;
            }

            if (_healthText != null)
            {
                _healthText.text = $"HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            if (_hudRoot != null && current < max)
            {
                _hudRoot.DOComplete();
                _hudRoot.DOShakePosition(0.25f, 8f).SetLink(gameObject);
            }
        }

        private void UpdateAmmoUI(WeaponConfig weapon, int currentAmmo, int reserveAmmo)
        {
            if (_weaponNameText != null)
            {
                _weaponNameText.text = weapon.WeaponName;
            }

            if (_ammoText != null)
            {
                if (weapon.ReloadType == ReloadType.Overheat)
                {
                    _ammoText.text = "HEAT: 0%";
                }
                else
                {
                    string reserve = weapon.InfiniteReserveAmmo ? "∞" : reserveAmmo.ToString();
                    _ammoText.text = $"{currentAmmo} / {reserve}";
                }
            }
        }

        private void UpdateHeatUI(float heatNormalized, bool isOverheated)
        {
            if (_ammoText != null)
            {
                int percent = Mathf.RoundToInt(heatNormalized * 100f);
                _ammoText.text = isOverheated ? $"OVERHEAT! ({percent}%)" : $"HEAT: {percent}%";
            }
        }

        private void UpdateReloadUI(bool isReloading)
        {
            if (_reloadStatusText != null)
            {
                _reloadStatusText.gameObject.SetActive(isReloading);
                _reloadStatusText.text = "ПЕРЕЗАРЯДКА...";
            }
        }

        private void UpdateInteractionPrompt(string prompt)
        {
            if (_interactionPromptText != null)
            {
                bool hasPrompt = !string.IsNullOrEmpty(prompt);
                _interactionPromptText.gameObject.SetActive(hasPrompt);
                _interactionPromptText.text = prompt;
            }
        }

        private void UpdateEnemiesRemaining(int count)
        {
            if (_enemiesRemainingText != null)
            {
                _enemiesRemainingText.text = count > 0
                    ? $"Врагов осталось: {count}"
                    : "Зона зачищена! Идите к выходу";
            }
        }

        public void TogglePause()
        {
            if (_isGameEnded)
            {
                return;
            }

            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            SetPanelState(_pausePanel, _isPaused);

            if (_isPaused)
            {
                _inputService.EnableUIInput();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _inputService.EnablePlayerInput();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void HandlePlayerDied()
        {
            if (_isGameEnded)
            {
                return;
            }

            _isGameEnded = true;
            Time.timeScale = 0f;
            _inputService.EnableUIInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetPanelState(_defeatPanel, true);
        }

        private void HandleLevelVictory()
        {
            if (_isGameEnded)
            {
                return;
            }

            _isGameEnded = true;
            Time.timeScale = 0f;
            _inputService.EnableUIInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetPanelState(_victoryPanel, true);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        private void SetPanelState(GameObject panel, bool isActive)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }
    }
}
