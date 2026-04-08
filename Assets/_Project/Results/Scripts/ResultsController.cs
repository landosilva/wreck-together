namespace WreckTogether.Results
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class ResultsController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private GameSessionData _gameSessionData;
        [SerializeField] private string _lobbySceneName = "Lobby";
        [SerializeField] private string _menuSceneName = "Menu";

        private Button _replayButton;
        private Button _menuButton;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            var resultLabel = root.Q<Label>("result-label");
            var scoreLabel = root.Q<Label>("score-label");
            _replayButton = root.Q<Button>("replay-button");
            _menuButton = root.Q<Button>("menu-button");

            resultLabel.text = "Time's Up!";
            scoreLabel.text = $"Score: {_gameSessionData.FinalScore}";

            _replayButton.clicked += OnReplayClicked;
            _menuButton.clicked += OnMenuClicked;
        }

        private void OnDisable()
        {
            _replayButton.clicked -= OnReplayClicked;
            _menuButton.clicked -= OnMenuClicked;
        }

        private async void OnReplayClicked()
        {
            _replayButton.SetEnabled(false);
            _menuButton.SetEnabled(false);
            await _sceneLoader.LoadSceneAsync(_lobbySceneName);
        }

        private async void OnMenuClicked()
        {
            _replayButton.SetEnabled(false);
            _menuButton.SetEnabled(false);
            _gameSessionData.Clear();
            await _sceneLoader.LoadSceneAsync(_menuSceneName);
        }
    }
}
