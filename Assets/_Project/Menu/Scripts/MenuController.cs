namespace WreckTogether.Menu
{
    using Eflatun.SceneReference;
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class MenuController : MonoBehaviour
    {
        [Tooltip("UI Document containing menu buttons and settings overlay.")]
        [SerializeField] private UIDocument _uiDocument;

        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private SceneLoader _sceneLoader;

        [Tooltip("Scene loaded when the player presses Play.")]
        [SerializeField] private SceneReference _lobbyScene;

        private Button _playButton;
        private Button _settingsButton;
        private Button _quitButton;
        private VisualElement _settingsOverlay;
        private Button _settingsBackButton;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            _playButton = root.Q<Button>("play-button");
            _settingsButton = root.Q<Button>("settings-button");
            _quitButton = root.Q<Button>("quit-button");
            _settingsOverlay = root.Q<VisualElement>("settings-overlay");
            _settingsBackButton = root.Q<Button>("settings-back-button");

            _playButton.clicked += OnPlayClicked;
            _settingsButton.clicked += OnSettingsClicked;
            _quitButton.clicked += OnQuitClicked;
            _settingsBackButton.clicked += OnSettingsBackClicked;

            _settingsOverlay.style.display = DisplayStyle.None;
        }

        private void OnDisable()
        {
            _playButton.clicked -= OnPlayClicked;
            _settingsButton.clicked -= OnSettingsClicked;
            _quitButton.clicked -= OnQuitClicked;
            _settingsBackButton.clicked -= OnSettingsBackClicked;
        }

        private async void OnPlayClicked()
        {
            _playButton.SetEnabled(false);
            await _sceneLoader.LoadSceneAsync(_lobbyScene.Name);
        }

        private void OnSettingsClicked()
        {
            _settingsOverlay.style.display = DisplayStyle.Flex;
        }

        private void OnSettingsBackClicked()
        {
            _settingsOverlay.style.display = DisplayStyle.None;
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
