namespace WreckTogether.Settings
{
    using UnityEngine;
    using UnityEngine.UIElements;

    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private const string VolumeKey = "WreckTogether.Volume";

        private Slider _volumeSlider;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _volumeSlider = root.Q<Slider>("volume-slider");

            if (_volumeSlider != null)
            {
                _volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
                LoadSettings();
            }
        }

        private void OnDisable()
        {
            if (_volumeSlider != null)
            {
                _volumeSlider.UnregisterValueChangedCallback(OnVolumeChanged);
            }
        }

        private void OnVolumeChanged(ChangeEvent<float> evt)
        {
            AudioListener.volume = evt.newValue;
            PlayerPrefs.SetFloat(VolumeKey, evt.newValue);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            var volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            _volumeSlider.SetValueWithoutNotify(volume);
            AudioListener.volume = volume;
        }
    }
}
