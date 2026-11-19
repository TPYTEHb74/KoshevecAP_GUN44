using System;
using Game.Level;
using Game.Services;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float _interactDistance = 3.5f;
        [SerializeField] private LayerMask _interactableMask;

        private IInputService _inputService;
        private Transform _cameraTransform;
        private IInteractable _currentTarget;

        public event Action<string> OnPromptChanged;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Awake()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            _inputService.OnInteractPressed += HandleInteract;
        }

        private void OnDisable()
        {
            _inputService.OnInteractPressed -= HandleInteract;
        }

        private void Update()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit,
                _interactDistance, _interactableMask, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable) && interactable.CanInteract)
                {
                    if (_currentTarget != interactable)
                    {
                        _currentTarget = interactable;
                        OnPromptChanged?.Invoke(_currentTarget.InteractionPrompt);
                    }

                    return;
                }
            }

            if (_currentTarget != null)
            {
                _currentTarget = null;
                OnPromptChanged?.Invoke(string.Empty);
            }
        }

        private void HandleInteract()
        {
            if (_currentTarget != null && _currentTarget.CanInteract)
            {
                _currentTarget.Interact(gameObject);
            }
        }
    }
}
