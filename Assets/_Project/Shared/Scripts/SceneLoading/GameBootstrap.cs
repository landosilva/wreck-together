namespace WreckTogether.Shared
{
    using UnityEngine;
    using WreckTogether.Tuning;

    /// <summary>
    /// Runs once before any scene loads — in Editor Play and in Build. Applies
    /// cross-scene initialization (perf policy, session state) so any scene can
    /// be the first scene without breaking. Bootstrap-only data is loaded from
    /// <c>Resources/Tuning/</c> because no scene exists to hold a reference yet.
    /// </summary>
    public static class GameBootstrap
    {
        private const string PerformanceTuningResourcePath = "Tuning/PerformanceTuning";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GameSession.Clear();

            var performance = Resources.Load<PerformanceTuning>(PerformanceTuningResourcePath);
            if (performance == null)
            {
                Debug.LogWarning($"[GameBootstrap] Missing PerformanceTuning at Resources/{PerformanceTuningResourcePath}");
                return;
            }
            PerformanceBootstrap.Apply(performance);
        }
    }
}
