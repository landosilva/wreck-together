namespace WreckTogether.Boot
{
    using System.Threading.Tasks;
    using Eflatun.SceneReference;
    using UnityEngine;
    using WreckTogether.Shared;
    using WreckTogether.Tuning;

    public class BootController : MonoBehaviour
    {
        [Tooltip("Handles scene transitions with fade.")]
        [SerializeField] private SceneLoader _sceneLoader;

        [Tooltip("Splash duration + downstream match values.")]
        [SerializeField] private MatchTuning _match;

        [Tooltip("This scene (Boot) — needed so SceneLoader can unload it.")]
        [SerializeField] private SceneReference _bootScene;

        [Tooltip("First interactive scene loaded after the splash.")]
        [SerializeField] private SceneReference _menuScene;

        private async void Start()
        {
            GameSession.Clear();

            await Task.Delay((int)(_match.SplashDurationSeconds * 1000));

            _sceneLoader.SetCurrentScene(_bootScene.Name);
            await _sceneLoader.LoadSceneAsync(_menuScene.Name);
        }
    }
}
