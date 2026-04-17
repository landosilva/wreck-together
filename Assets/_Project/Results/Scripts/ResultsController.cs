namespace WreckTogether.Results
{
    using Eflatun.SceneReference;
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class ResultsController : MonoBehaviour
    {
        [Tooltip("UI Document containing result labels and navigation buttons.")]
        [SerializeField] private UIDocument _uiDocument;

        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private SceneLoader _sceneLoader;

        [Tooltip("Scene loaded when the player chooses to replay.")]
        [SerializeField] private SceneReference _lobbyScene;

        [Tooltip("Scene loaded when the player returns to main menu.")]
        [SerializeField] private SceneReference _menuScene;

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
            scoreLabel.text = $"Score: {GameSession.FinalScore}";

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
            await _sceneLoader.LoadSceneAsync(_lobbyScene.Name);
        }

        private async void OnMenuClicked()
        {
            _replayButton.SetEnabled(false);
            _menuButton.SetEnabled(false);
            GameSession.Clear();
            await _sceneLoader.LoadSceneAsync(_menuScene.Name);
        }
    }
}
