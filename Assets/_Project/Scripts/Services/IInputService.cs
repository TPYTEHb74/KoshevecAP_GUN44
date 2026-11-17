using System;
using UnityEngine;

namespace Game.Services
{
    public interface IInputService
    {
        Vector2 MoveAxis { get; }
        Vector2 LookAxis { get; }
        bool IsSprinting { get; }
        bool IsAiming { get; }

        event Action OnJumpPressed;
        event Action OnCrouchPressed;
        event Action OnFirePressed;
        event Action OnFireReleased;
        event Action OnReloadPressed;
        event Action OnMeleePressed;
        event Action OnInteractPressed;
        event Action<int> OnWeaponSlotSelected;
        event Action<float> OnWeaponScrolled;
        event Action OnToggleMapPressed;
        event Action OnPausePressed;

        void EnablePlayerInput();
        void EnableUIInput();
        void DisableAllInput();
    }
}
