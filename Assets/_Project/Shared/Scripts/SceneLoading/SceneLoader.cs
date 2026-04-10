namespace WreckTogether.Shared
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;
    using System.Threading.Tasks;

    [CreateAssetMenu(menuName = "WreckTogether/Scene Loader")]
    public class SceneLoader : ScriptableObject, ISceneLoader
    {
        [SerializeField] private PanelSettings _panelSettings;
        [SerializeField] private float _fadeDuration = 0.3f;

        private string _currentScene;
        private VisualElement _overlay;
        private UIDocument _overlayDocument;

        public async Task LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(_currentScene))
            {
                _currentScene = SceneManager.GetActiveScene().name;
            }

            var previousScene = _currentScene;

            EnsureOverlay();

            // Fade to black
            await FadeAsync(1f);

            // Load the new scene
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene: {sceneName}");
                await FadeAsync(0f);
                return;
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            _currentScene = sceneName;

            // Unload previous scene while still black
            if (!string.IsNullOrEmpty(previousScene))
            {
                await UnloadSceneAsync(previousScene);
            }

            // Fade from black
            await FadeAsync(0f);
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

        public void SetCurrentScene(string sceneName)
        {
            _currentScene = sceneName;
        }

        private void EnsureOverlay()
        {
            if (_overlay != null) return;

            var go = new GameObject("[SceneTransition]");
            Object.DontDestroyOnLoad(go);

            _overlayDocument = go.AddComponent<UIDocument>();
            _overlayDocument.panelSettings = _panelSettings;
            _overlayDocument.sortingOrder = 1000;

            _overlay = new VisualElement();
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0;
            _overlay.style.top = 0;
            _overlay.style.right = 0;
            _overlay.style.bottom = 0;
            _overlay.style.backgroundColor = Color.black;
            _overlay.style.opacity = 0f;
            _overlay.pickingMode = PickingMode.Ignore;
            _overlay.style.transitionProperty = new System.Collections.Generic.List<StylePropertyName>
            {
                new StylePropertyName("opacity")
            };
            _overlay.style.transitionDuration = new System.Collections.Generic.List<TimeValue>
            {
                new TimeValue(_fadeDuration * 1000f, TimeUnit.Millisecond)
            };

            _overlayDocument.rootVisualElement.Add(_overlay);
        }

        private async Task FadeAsync(float targetOpacity)
        {
            if (_overlay == null) return;

            _overlay.pickingMode = targetOpacity > 0.5f ? PickingMode.Position : PickingMode.Ignore;
            _overlay.style.opacity = targetOpacity;

            // Wait for the CSS transition to complete
            var ms = (int)(_fadeDuration * 1000f) + 50;
            await Task.Delay(ms);
        }

        private void OnEnable()
        {
            _currentScene = null;
            _overlay = null;
            _overlayDocument = null;
        }
    }
}
