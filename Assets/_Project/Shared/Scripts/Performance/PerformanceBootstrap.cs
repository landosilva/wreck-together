namespace WreckTogether.Shared
{
    using UnityEngine;
    using WreckTogether.Tuning;

    /// <summary>
    /// Single entry point that owns frame-rate + quality-level policy.
    /// Use <see cref="Apply"/> once at boot, then <see cref="SetQualityLevel"/>
    /// whenever entering a scene that needs a different quality tier.
    /// </summary>
    public static class PerformanceBootstrap
    {
        private static PerformanceTuning _tuning;

        public static void Apply(PerformanceTuning tuning)
        {
            _tuning = tuning;
            SetQualityLevel(tuning.UIQualityLevel);
        }

        public static void SetQualityLevel(int level)
        {
            QualitySettings.SetQualityLevel(level, applyExpensiveChanges: true);
            EnforceFrameRatePolicy();
        }

        // vSync must be 0 for Application.targetFrameRate to take effect.
        // Medium+ quality presets enable vSync, which would sync to the display
        // refresh rate (120Hz+ on ProMotion) and ignore our cap.
        private static void EnforceFrameRatePolicy()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _tuning != null ? _tuning.TargetFrameRate : 60;
        }
    }
}
