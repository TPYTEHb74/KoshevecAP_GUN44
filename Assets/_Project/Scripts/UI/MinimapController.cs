using DG.Tweening;
using Game.Player;
using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public class MinimapController : MonoBehaviour
    {
        [Header("Cameras & UI")]
        [SerializeField] private Camera _minimapCamera;
        [SerializeField] private RectTransform _minimapContainer;
        [SerializeField] private RectTransform _fullMapPanel;
        [SerializeField] private float _minimapHeight = 25f;
        [SerializeField] private float _smallOrthoSize = 15f;
        [SerializeField] private float _fullMapOrthoSize = 40f;

        private IInputService _inputService;
        private PlayerController _player;
        private bool _isFullMapOpen;

        [Inject]
        public void Construct(IInputService inputService, PlayerController player)
        {
            _inputService = inputService;
            _player = player;
        }

        private void OnEnable()
        {
            _inputService.OnToggleMapPressed += ToggleFullMap;
        }

        private void OnDisable()
        {
            _inputService.OnToggleMapPressed -= ToggleFullMap;
        }

        private void Start()
        {
            if (_fullMapPanel != null)
            {
                _fullMapPanel.gameObject.SetActive(false);
            }

            if (_minimapCamera != null)
            {
                _minimapCamera.orthographicSize = _smallOrthoSize;
            }
        }

        private void LateUpdate()
        {
            if (_minimapCamera == null || _player == null)
            {
                return;
            }

            if (!_isFullMapOpen)
            {
                Vector3 playerPos = _player.transform.position;
                _minimapCamera.transform.position = new Vector3(playerPos.x, playerPos.y + _minimapHeight, playerPos.z);
                _minimapCamera.transform.rotation = Quaternion.Euler(90f, _player.transform.eulerAngles.y, 0f);
            }
            else
            {
                _minimapCamera.transform.position = new Vector3(0f, _minimapHeight * 1.5f, 35f);
                _minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private void ToggleFullMap()
        {
            _isFullMapOpen = !_isFullMapOpen;

            if (_minimapCamera != null)
            {
                _minimapCamera.orthographicSize = _isFullMapOpen ? _fullMapOrthoSize : _smallOrthoSize;
            }

            if (_minimapContainer != null)
            {
                _minimapContainer.gameObject.SetActive(!_isFullMapOpen);
            }

            if (_fullMapPanel != null)
            {
                _fullMapPanel.gameObject.SetActive(_isFullMapOpen);
                if (_isFullMapOpen)
                {
                    _fullMapPanel.localScale = Vector3.one * 0.85f;
                    _fullMapPanel.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
                }
            }
        }
    }
}
