namespace WreckTogether.Tuning
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Tuning/Match", fileName = "MatchTuning")]
    public class MatchTuning : ScriptableObject
    {
        [Tooltip("Total match duration in seconds. Source of truth for the match clock.")]
        public float MatchDurationSeconds = 300f;

        [Tooltip("How long the splash screen stays visible before navigating to menu.")]
        public float SplashDurationSeconds = 2f;
    }
}
