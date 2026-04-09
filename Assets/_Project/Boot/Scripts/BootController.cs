namespace WreckTogether.Boot
{
    using Eflatun.SceneReference;
    using UnityEngine;
    using WreckTogether.Shared;

    public class BootController : MonoBehaviour
    {
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private GameSessionData _gameSessionData;
        [SerializeField] private float _splashDuration = 2f;
        [SerializeField] private SceneReference _bootScene;
        [SerializeField] private SceneReference _menuScene;

        private async void Start()
        {
            _gameSessionData.Clear();

            await System.Threading.Tasks.Task.Delay(
                (int)(_splashDuration * 1000));

            _sceneLoader.SetCurrentScene(_bootScene.Name);
            await _sceneLoader.LoadSceneAsync(_menuScene.Name);
        }
    }
}
