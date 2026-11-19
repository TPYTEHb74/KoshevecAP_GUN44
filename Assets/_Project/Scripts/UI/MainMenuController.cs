using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _loadingBar;
        [SerializeField] private int _gameplaySceneIndex = 1;

        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_inputService != null)
            {
                _inputService.EnableUIInput();
            }

            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
        }

        public void StartGame()
        {
            LoadGameplayAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private async UniTaskVoid LoadGameplayAsync(CancellationToken token)
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(_gameplaySceneIndex);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                if (_loadingBar != null)
                {
                    _loadingBar.value = Mathf.Clamp01(operation.progress / 0.9f);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (_loadingBar != null)
            {
                _loadingBar.value = 1f;
            }

            await UniTask.Delay(300, cancellationToken: token);
            operation.allowSceneActivation = true;
        }
    }
}
