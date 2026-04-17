namespace WreckTogether.Tuning
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Tuning/Performance", fileName = "PerformanceTuning")]
    public class PerformanceTuning : ScriptableObject
    {
        [Tooltip("Application.targetFrameRate value applied at boot and whenever quality changes.")]
        public int TargetFrameRate = 60;

        [Tooltip("Quality level used on UI-only scenes (menu, lobby, results).")]
        public int UIQualityLevel = 2;

        [Tooltip("Quality level applied when entering a gameplay scene.")]
        public int GameplayQualityLevel = 5;
    }
}
