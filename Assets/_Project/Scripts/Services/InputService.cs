using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Services
{
    public class InputService : IInputService, IInitializable, IDisposable
    {
        private readonly GameInputActions _inputActions;

        public Vector2 MoveAxis => _inputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookAxis => _inputActions.Player.Look.ReadValue<Vector2>();
        public bool IsSprinting => _inputActions.Player.Sprint.IsPressed();
        public bool IsAiming => _inputActions.Player.Aim.IsPressed();

        public event Action OnJumpPressed;
        public event Action OnCrouchPressed;
        public event Action OnFirePressed;
        public event Action OnFireReleased;
        public event Action OnReloadPressed;
        public event Action OnMeleePressed;
        public event Action OnInteractPressed;
        public event Action<int> OnWeaponSlotSelected;
        public event Action<float> OnWeaponScrolled;
        public event Action OnToggleMapPressed;
        public event Action OnPausePressed;

        public InputService()
        {
            _inputActions = new GameInputActions();
        }

        public void Initialize()
        {
            _inputActions.Player.Jump.performed += HandleJump;
            _inputActions.Player.Crouch.performed += HandleCrouch;
            _inputActions.Player.Fire.started += HandleFireStarted;
            _inputActions.Player.Fire.canceled += HandleFireCanceled;
            _inputActions.Player.Reload.performed += HandleReload;
            _inputActions.Player.Melee.performed += HandleMelee;
            _inputActions.Player.Interact.performed += HandleInteract;
            _inputActions.Player.SwitchWeapon.performed += HandleWeaponScroll;
            _inputActions.Player.Pause.performed += HandlePause;
            _inputActions.Player.ToggleMap.performed += HandleToggleMap;

            _inputActions.UI.Resume.performed += HandlePause;
            _inputActions.UI.CloseMap.performed += HandleToggleMap;

            _inputActions.Player.Weapon1.performed += _ => OnWeaponSlotSelected?.Invoke(0);
            _inputActions.Player.Weapon2.performed += _ => OnWeaponSlotSelected?.Invoke(1);
            _inputActions.Player.Weapon3.performed += _ => OnWeaponSlotSelected?.Invoke(2);
            _inputActions.Player.Weapon4.performed += _ => OnWeaponSlotSelected?.Invoke(3);
            _inputActions.Player.Weapon5.performed += _ => OnWeaponSlotSelected?.Invoke(4);
        }

        public void Dispose()
        {
            _inputActions.Player.Jump.performed -= HandleJump;
            _inputActions.Player.Crouch.performed -= HandleCrouch;
            _inputActions.Player.Fire.started -= HandleFireStarted;
            _inputActions.Player.Fire.canceled -= HandleFireCanceled;
            _inputActions.Player.Reload.performed -= HandleReload;
            _inputActions.Player.Melee.performed -= HandleMelee;
            _inputActions.Player.Interact.performed -= HandleInteract;
            _inputActions.Player.SwitchWeapon.performed -= HandleWeaponScroll;
            _inputActions.Player.Pause.performed -= HandlePause;
            _inputActions.Player.ToggleMap.performed -= HandleToggleMap;

            _inputActions.UI.Resume.performed -= HandlePause;
            _inputActions.UI.CloseMap.performed -= HandleToggleMap;

            _inputActions.Disable();
            _inputActions.Dispose();
        }

        public void EnablePlayerInput()
        {
            _inputActions.UI.Disable();
            _inputActions.Player.Enable();
        }

        public void EnableUIInput()
        {
            _inputActions.Player.Disable();
            _inputActions.UI.Enable();
        }

        public void DisableAllInput()
        {
            _inputActions.Player.Disable();
            _inputActions.UI.Disable();
        }

        private void HandleJump(InputAction.CallbackContext context) => OnJumpPressed?.Invoke();
        private void HandleCrouch(InputAction.CallbackContext context) => OnCrouchPressed?.Invoke();
        private void HandleFireStarted(InputAction.CallbackContext context) => OnFirePressed?.Invoke();
        private void HandleFireCanceled(InputAction.CallbackContext context) => OnFireReleased?.Invoke();
        private void HandleReload(InputAction.CallbackContext context) => OnReloadPressed?.Invoke();
        private void HandleMelee(InputAction.CallbackContext context) => OnMeleePressed?.Invoke();
        private void HandleInteract(InputAction.CallbackContext context) => OnInteractPressed?.Invoke();
        private void HandlePause(InputAction.CallbackContext context) => OnPausePressed?.Invoke();
        private void HandleToggleMap(InputAction.CallbackContext context) => OnToggleMapPressed?.Invoke();
        private void HandleWeaponScroll(InputAction.CallbackContext context) => OnWeaponScrolled?.Invoke(context.ReadValue<float>());
    }
}
