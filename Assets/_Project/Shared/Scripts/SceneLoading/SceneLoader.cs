namespace WreckTogether.Shared
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System.Threading.Tasks;

    [CreateAssetMenu(menuName = "WreckTogether/Scene Loader")]
    public class SceneLoader : ScriptableObject, ISceneLoader
    {
        private string _currentScene;

        public async Task LoadSceneAsync(string sceneName)
        {
            if (!string.IsNullOrEmpty(_currentScene))
            {
                await UnloadSceneAsync(_currentScene);
            }

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene: {sceneName}");
                return;
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            _currentScene = sceneName;
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                Debug.LogWarning($"[SceneLoader] Scene not loaded: {sceneName}");
                return;
            }

            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null)
            {
                return;
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (_currentScene == sceneName)
            {
                _currentScene = null;
            }
        }

        private void OnEnable()
        {
            _currentScene = null;
        }
    }
}
