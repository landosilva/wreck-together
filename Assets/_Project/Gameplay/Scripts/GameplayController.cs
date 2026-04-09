namespace WreckTogether.Gameplay
{
    using Eflatun.SceneReference;
    using Quantum;
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private GameSessionData _gameSessionData;
        [SerializeField] private SceneReference _resultsScene;

        private Label _timerLabel;
        private bool _transitioning;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _timerLabel = root.Q<Label>("timer-label");

            QuantumCallback.Subscribe(this, (CallbackUpdateView callback) => OnUpdateView(callback));
        }

        private unsafe void OnUpdateView(CallbackUpdateView callback)
        {
            if (_transitioning)
            {
                return;
            }

            var frame = callback.Game.Frames.Verified;
            if (frame == null)
            {
                return;
            }

            var timer = frame.Global->MatchTimer.AsFloat;
            var matchOver = frame.Global->MatchOver;

            _timerLabel.text = Mathf.CeilToInt(timer).ToString();

            if (matchOver)
            {
                _transitioning = true;
                OnMatchOver(callback.Game);
            }
        }

        private async void OnMatchOver(QuantumGame game)
        {
            _gameSessionData.FinalScore = 0; // placeholder
            _gameSessionData.MatchDuration = 30f;

            // Shutdown Quantum
            var runner = QuantumRunner.FindRunner(game);
            if (runner != null)
            {
                await runner.ShutdownAsync();
            }

            await _sceneLoader.LoadSceneAsync(_resultsScene.Name);
        }
    }
}
