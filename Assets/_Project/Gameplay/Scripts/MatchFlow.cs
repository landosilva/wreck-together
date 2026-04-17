namespace WreckTogether.Gameplay
{
    using Eflatun.SceneReference;
    using Quantum;
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;
    using WreckTogether.Tuning;

    public class MatchFlow : MonoBehaviour
    {
        [Tooltip("UI Document containing the timer-label element.")]
        [SerializeField] private UIDocument _uiDocument;

        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private SceneLoader _sceneLoader;

        [Tooltip("Scene loaded when the match ends.")]
        [SerializeField] private SceneReference _resultsScene;

        [Tooltip("Match duration + results-screen value.")]
        [SerializeField] private MatchTuning _match;

        [Tooltip("Quality level applied when entering gameplay.")]
        [SerializeField] private PerformanceTuning _performance;

        private Label _timerLabel;
        private bool _transitioning;

        private void OnEnable()
        {
            PerformanceBootstrap.SetQualityLevel(_performance.GameplayQualityLevel);
            _timerLabel = _uiDocument.rootVisualElement.Q<Label>("timer-label");
            QuantumCallback.Subscribe(this, (CallbackUpdateView cb) => OnUpdateView(cb));
        }

        private unsafe void OnUpdateView(CallbackUpdateView callback)
        {
            if (_transitioning) return;

            var frame = callback.Game.Frames.Verified;
            if (frame == null) return;

            RenderTimer(frame.Global->MatchTimer.AsFloat);

            if (frame.Global->MatchOver)
            {
                _transitioning = true;
                TransitionToResults(callback.Game);
            }
        }

        private void RenderTimer(float secondsRemaining)
        {
            var totalSeconds = Mathf.CeilToInt(secondsRemaining);
            _timerLabel.text = $"{totalSeconds / 60}:{totalSeconds % 60:00}";
        }

        private async void TransitionToResults(QuantumGame game)
        {
            GameSession.FinalScore = 0;
            GameSession.MatchDuration = _match.MatchDurationSeconds;

            var runner = QuantumRunner.FindRunner(game);
            if (runner != null) await runner.ShutdownAsync();

            await _sceneLoader.LoadSceneAsync(_resultsScene.Name);
        }
    }
}
