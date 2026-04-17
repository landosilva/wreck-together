namespace WreckTogether.Gameplay
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using WreckTogether.Shared;

    /// <summary>
    /// Binds UI controls to <see cref="PlayerSettings"/>. Each control is optional
    /// so the same component works on any screen that has a subset of the controls.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Tooltip("UI Document containing settings controls (sliders, toggles).")]
        [SerializeField] private UIDocument _uiDocument;

        private Slider _sensitivity;
        private Toggle _invertY;
        private Toggle _fovScale;
        private Slider _gamepadLook;
        private Slider _masterVolume;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _sensitivity  = root.Q<Slider>("sensitivity-slider");
            _invertY      = root.Q<Toggle>("invert-y-toggle");
            _fovScale     = root.Q<Toggle>("fov-scale-toggle");
            _gamepadLook  = root.Q<Slider>("gamepad-sens-slider");
            _masterVolume = root.Q<Slider>("volume-slider");

            InitializeFromSettings();
            RegisterCallbacks();
            AudioListener.volume = PlayerSettings.MasterVolume;
        }

        private void OnDisable() => UnregisterCallbacks();

        private void InitializeFromSettings()
        {
            if (_sensitivity  != null) _sensitivity.value  = PlayerSettings.MouseDegreesPerCount;
            if (_invertY      != null) _invertY.value      = PlayerSettings.InvertMouseY;
            if (_fovScale     != null) _fovScale.value     = PlayerSettings.ScaleSensitivityWithFOV;
            if (_gamepadLook  != null) _gamepadLook.value  = PlayerSettings.GamepadLookSensitivity;
            if (_masterVolume != null) _masterVolume.value = PlayerSettings.MasterVolume;
        }

        private void RegisterCallbacks()
        {
            _sensitivity ?.RegisterValueChangedCallback(OnSensitivity);
            _invertY     ?.RegisterValueChangedCallback(OnInvertY);
            _fovScale    ?.RegisterValueChangedCallback(OnFovScale);
            _gamepadLook ?.RegisterValueChangedCallback(OnGamepadLook);
            _masterVolume?.RegisterValueChangedCallback(OnMasterVolume);
        }

        private void UnregisterCallbacks()
        {
            _sensitivity ?.UnregisterValueChangedCallback(OnSensitivity);
            _invertY     ?.UnregisterValueChangedCallback(OnInvertY);
            _fovScale    ?.UnregisterValueChangedCallback(OnFovScale);
            _gamepadLook ?.UnregisterValueChangedCallback(OnGamepadLook);
            _masterVolume?.UnregisterValueChangedCallback(OnMasterVolume);
        }

        private void OnSensitivity(ChangeEvent<float> e) => PlayerSettings.MouseDegreesPerCount = e.newValue;
        private void OnInvertY(ChangeEvent<bool> e)      => PlayerSettings.InvertMouseY = e.newValue;
        private void OnFovScale(ChangeEvent<bool> e)     => PlayerSettings.ScaleSensitivityWithFOV = e.newValue;
        private void OnGamepadLook(ChangeEvent<float> e) => PlayerSettings.GamepadLookSensitivity = e.newValue;

        private void OnMasterVolume(ChangeEvent<float> e)
        {
            PlayerSettings.MasterVolume = e.newValue;
            AudioListener.volume = e.newValue;
        }
    }
}
