namespace WreckTogether.Menu
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class MenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private string _lobbySceneName = "Lobby";

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
            await _sceneLoader.LoadSceneAsync(_lobbySceneName);
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
