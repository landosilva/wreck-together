namespace WreckTogether.Shared
{
    using System;
    using UnityEngine;

    /// <summary>
    /// User-tunable settings, persisted via PlayerPrefs. Pure storage — any
    /// domain-specific math (e.g. FOV sensitivity scaling) lives with its caller.
    /// </summary>
    public static class PlayerSettings
    {
        // Defaults duplicated from InputTuning so PlayerSettings stays a zero-dependency
        // static. The live designer-authored values come from InputTuning at runtime;
        // these only apply when the user has never touched the setting.
        public const float DefaultMouseDegreesPerCount = 0.15f;
        public const bool  DefaultInvertMouseY         = false;
        public const bool  DefaultScaleSensWithFOV     = true;
        public const float DefaultGamepadLookSens      = 120f;
        public const float DefaultMasterVolume         = 1f;

        private const string KeyMouseDegPerCount = "wt.input.mouseDegreesPerCount";
        private const string KeyInvertMouseY     = "wt.input.invertMouseY";
        private const string KeyScaleSensWithFOV = "wt.input.scaleSensWithFOV";
        private const string KeyGamepadLookSens  = "wt.input.gamepadLookSensitivity";
        private const string KeyMasterVolume     = "wt.audio.masterVolume";

        public static event Action Changed;

        public static float MouseDegreesPerCount
        {
            get => PlayerPrefs.GetFloat(KeyMouseDegPerCount, DefaultMouseDegreesPerCount);
            set => SetFloat(KeyMouseDegPerCount, value);
        }

        public static bool InvertMouseY
        {
            get => GetBool(KeyInvertMouseY, DefaultInvertMouseY);
            set => SetBool(KeyInvertMouseY, value);
        }

        public static bool ScaleSensitivityWithFOV
        {
            get => GetBool(KeyScaleSensWithFOV, DefaultScaleSensWithFOV);
            set => SetBool(KeyScaleSensWithFOV, value);
        }

        public static float GamepadLookSensitivity
        {
            get => PlayerPrefs.GetFloat(KeyGamepadLookSens, DefaultGamepadLookSens);
            set => SetFloat(KeyGamepadLookSens, value);
        }

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(KeyMasterVolume, DefaultMasterVolume);
            set => SetFloat(KeyMasterVolume, value);
        }

        public static void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(KeyMouseDegPerCount);
            PlayerPrefs.DeleteKey(KeyInvertMouseY);
            PlayerPrefs.DeleteKey(KeyScaleSensWithFOV);
            PlayerPrefs.DeleteKey(KeyGamepadLookSens);
            PlayerPrefs.DeleteKey(KeyMasterVolume);
            Notify();
        }

        private static bool GetBool(string key, bool fallback) => PlayerPrefs.GetInt(key, fallback ? 1 : 0) != 0;

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            Notify();
        }

        private static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            Notify();
        }

        private static void Notify()
        {
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
