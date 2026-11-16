using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleController : MonoBehaviour
{
    [SerializeField] private Battlefield _battlefield;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Slider _restartSlider;

    private GameInput _input;
    private IGameplayCommand _command;
    private Cell _hoveredCell;
    private bool _isHoldingRestart;
    private float _holdTimer;
    private float _holdDuration = 1.5f;

    private void Awake()
    {
        InitInput();
        _command = new CheckersCommand(_battlefield, _playerController);

        if (_restartSlider != null)
        {
            _restartSlider.value = 0f;
            _restartSlider.gameObject.SetActive(false);
        }
    }

    private void InitInput()
    {
        if (_input == null)
        {
            _input = new GameInput();
        }
    }

    private void OnEnable()
    {
        InitInput();
        _input.Gameplay.Enable();
        _input.Gameplay.Select.performed += OnSelect;
        _input.Gameplay.Cancel.performed += OnCancel;
        _input.Gameplay.Confirm.performed += OnConfirm;
        _input.Gameplay.Restart.started += OnRestartStarted;
        _input.Gameplay.Restart.canceled += OnRestartCanceled;
    }

    private void OnDisable()
    {
        if (_input == null) return;

        _input.Gameplay.Select.performed -= OnSelect;
        _input.Gameplay.Cancel.performed -= OnCancel;
        _input.Gameplay.Confirm.performed -= OnConfirm;
        _input.Gameplay.Restart.started -= OnRestartStarted;
        _input.Gameplay.Restart.canceled -= OnRestartCanceled;
        _input.Gameplay.Disable();
    }

    private void Update()
    {
        if (_isHoldingRestart)
        {
            _holdTimer += Time.deltaTime;
            if (_restartSlider != null)
            {
                _restartSlider.gameObject.SetActive(true);
                _restartSlider.value = _holdTimer / _holdDuration;
            }

            if (_holdTimer >= _holdDuration)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    private void OnRestartStarted(InputAction.CallbackContext context)
    {
        _isHoldingRestart = true;
        _holdTimer = 0f;
    }

    private void OnRestartCanceled(InputAction.CallbackContext context)
    {
        _isHoldingRestart = false;
        _holdTimer = 0f;
        if (_restartSlider != null)
        {
            _restartSlider.value = 0f;
            _restartSlider.gameObject.SetActive(false);
        }
    }

    public void SetHoveredCell(Cell cell)
    {
        _hoveredCell = cell;
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        if (_playerController.IsBusy) return;
        if (_hoveredCell != null)
        {
            _command.Interact(_hoveredCell);
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (_playerController.IsBusy) return;
        _command.Cancel();
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (_playerController.IsBusy) return;
        _command.Confirm();
    }
}
