namespace WreckTogether.Tuning
{
    using Quantum;
    using UnityEngine;

    /// <summary>
    /// Quantum assets needed to start a match session: map, game config, player
    /// prototype. Referenced by both the lobby flow and the offline bootstrap so
    /// both code paths agree on what "the current match" consists of.
    /// </summary>
    [CreateAssetMenu(menuName = "WreckTogether/Tuning/Session Assets", fileName = "SessionAssets")]
    public class SessionAssets : ScriptableObject
    {
        [Tooltip("Quantum map asset used by the gameplay session.")]
        public AssetRef<Map> Map;

        [Tooltip("Quantum game config driving match rules.")]
        public AssetRef<Quantum.WreckTogether.WreckGameConfig> WreckGameConfig;

        [Tooltip("Entity prototype spawned for each player.")]
        public AssetRef<EntityPrototype> PlayerPrototype;
    }
}
