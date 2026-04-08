namespace WreckTogether.Boot
{
    using UnityEngine;
    using WreckTogether.Shared;

    public class BootController : MonoBehaviour
    {
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private GameSessionData _gameSessionData;
        [SerializeField] private float _splashDuration = 2f;
        [SerializeField] private string _menuSceneName = "Menu";

        private async void Start()
        {
            _gameSessionData.Clear();

            await System.Threading.Tasks.Task.Delay(
                (int)(_splashDuration * 1000));

            await _sceneLoader.LoadSceneAsync(_menuSceneName);
        }
    }
}
