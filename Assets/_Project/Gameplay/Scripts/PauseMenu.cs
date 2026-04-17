namespace WreckTogether.Gameplay
{
    using Eflatun.SceneReference;
    using Quantum;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;
    using UIButton = UnityEngine.UIElements.Button;

    public class PauseMenu : MonoBehaviour
    {
        [Tooltip("UI Document containing the pause-overlay element.")]
        [SerializeField] private UIDocument _uiDocument;

        [Tooltip("Look input to freeze while paused.")]
        [SerializeField] private GameplayInput _input;

        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private SceneLoader _sceneLoader;

        [Tooltip("Scene loaded when the player quits to menu.")]
        [SerializeField] private SceneReference _menuScene;

        private VisualElement _overlay;
        private UIButton _resume;
        private UIButton _quitToMenu;
        private UIButton _quitGame;
        private bool _isPaused;
        private bool _quitting;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _overlay    = root.Q<VisualElement>("pause-overlay");
            _resume     = root.Q<UIButton>("resume-button");
            _quitToMenu = root.Q<UIButton>("quit-to-menu-button");
            _quitGame   = root.Q<UIButton>("quit-game-button");

            _resume.clicked     += Resume;
            _quitToMenu.clicked += QuitToMenu;
            _quitGame.clicked   += QuitGame;

            SetPaused(false);
        }

        private void OnDisable()
        {
            if (_resume     != null) _resume.clicked     -= Resume;
            if (_quitToMenu != null) _quitToMenu.clicked -= QuitToMenu;
            if (_quitGame   != null) _quitGame.clicked   -= QuitGame;
        }

        private void Update()
        {
            if (_quitting) return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                SetPaused(!_isPaused);
        }

        private void Resume() => SetPaused(false);

        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            _overlay.style.display = paused ? DisplayStyle.Flex : DisplayStyle.None;
            _input.LookEnabled = !paused;
        }

        private async void QuitToMenu()
        {
            if (_quitting) return;
            _quitting = true;

            var runner = QuantumRunner.Default;
            if (runner != null) await runner.ShutdownAsync();

            await _sceneLoader.LoadSceneAsync(_menuScene.Name);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
